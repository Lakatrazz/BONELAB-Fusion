using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Syncables;
using LabFusion.Patching;

using SLZ;
using SLZ.Interaction;
using LabFusion.Extensions;

namespace LabFusion.Network
{
    public class MagazineInsertData : IFusionSerializable, IDisposable
    {
        public byte smallId;
        public ushort magazineId;
        public ushort gunId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(magazineId);
            writer.Write(gunId);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            magazineId = reader.ReadUInt16();
            gunId = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static MagazineInsertData Create(byte smallId, ushort magazineId, ushort gunId)
        {
            return new MagazineInsertData()
            {
                smallId = smallId,
                magazineId = magazineId,
                gunId = gunId,
            };
        }
    }

    [Net.DelayWhileLoading]
    public class MagazineInsertMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.MagazineInsert;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<MagazineInsertData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else {
                        if (SyncManager.TryGetSyncable(data.magazineId, out var mag) && mag is PropSyncable magazineSyncable && SyncManager.TryGetSyncable(data.gunId, out var gun) && gun is PropSyncable gunSyncable) {
                            // Insert mag into gun
                            if (magazineSyncable.Magazine && gunSyncable.Gun) {
                                if (gunSyncable.AmmoSocket._magazinePlug) {
                                    var otherPlug = gunSyncable.AmmoSocket._magazinePlug;

                                    if (otherPlug != magazineSyncable.Magazine.magazinePlug) {
                                        AmmoSocketPatches.IgnorePatch = true;
                                        if (otherPlug)
                                        {
                                            otherPlug.ForceEject();

                                            if (PropSyncable.MagazineCache.TryGet(otherPlug.magazine, out var otherMag))
                                            {
                                                otherMag.SetRigidbodiesDirty();
                                            }
                                        }
                                        AmmoSocketPatches.IgnorePatch = false;
                                    }
                                }

                                AmmoSocketPatches.IgnorePatch = true;
                                magazineSyncable.Magazine.magazinePlug.InsertPlug(gunSyncable.AmmoSocket);
                                AmmoSocketPatches.IgnorePatch = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
