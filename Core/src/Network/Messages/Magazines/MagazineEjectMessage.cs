using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Patching;
using LabFusion.Extensions;

using SLZ;

namespace LabFusion.Network
{
    public class MagazineEjectData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) * 2 + sizeof(ushort) * 2;

        public byte smallId;
        public ushort magazineId;
        public ushort gunId;
        public Handedness hand;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(magazineId);
            writer.Write(gunId);
            writer.Write((byte)hand);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            magazineId = reader.ReadUInt16();
            gunId = reader.ReadUInt16();
            hand = (Handedness)reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static MagazineEjectData Create(byte smallId, ushort magazineId, ushort gunId, Handedness hand)
        {
            return new MagazineEjectData()
            {
                smallId = smallId,
                magazineId = magazineId,
                gunId = gunId,
                hand = hand,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class MagazineEjectMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.MagazineEject;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<MagazineEjectData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else if (SyncManager.TryGetSyncable(data.gunId, out var gun) && gun is PropSyncable gunSyncable && gunSyncable.TryGetExtender<AmmoSocketExtender>(out var extender)) {
                        // Eject mag from gun
                        if (extender.Component._magazinePlug) {
                            AmmoSocketPatches.IgnorePatch = true;

                            var ammoPlug = extender.Component._magazinePlug;
                            if (ammoPlug.magazine && MagazineExtender.Cache.TryGet(ammoPlug.magazine, out var magSyncable) && magSyncable.Id == data.magazineId) {
                                ammoPlug.ForceEject();

                                magSyncable.SetRigidbodiesDirty();

                                if (data.hand != Handedness.UNDEFINED && PlayerRepManager.TryGetPlayerRep(data.smallId, out var rep)) {
                                    rep.AttachObject(data.hand, ammoPlug.magazine.grip);
                                }
                            }

                            AmmoSocketPatches.IgnorePatch = false;
                        }
                    }
                }
            }
        }
    }
}
