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
using SLZ.Props.Weapons;
using UnityEngine;
using SLZ.Marrow.Data;
using LabFusion.Extensions;

namespace LabFusion.Network
{
    public class ObjectDestructableDestroyData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) * 2 + sizeof(ushort);

        public byte smallId;
        public ushort syncId;
        public byte destructableIndex;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(syncId);
            writer.Write(destructableIndex);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            syncId = reader.ReadUInt16();
            destructableIndex = reader.ReadByte();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static ObjectDestructableDestroyData Create(byte smallId, ushort syncId, byte destructableIndex)
        {
            return new ObjectDestructableDestroyData()
            {
                smallId = smallId,
                syncId = syncId,
                destructableIndex = destructableIndex,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class ObjectDestructableDestroyMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.ObjectDestructableDestroy;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (FusionReader reader = FusionReader.Create(bytes))
            {
                using (var data = reader.ReadFusionSerializable<ObjectDestructableDestroyData>())
                {
                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.smallId, NetworkChannel.Reliable, message, false);
                        }
                    }
                    else
                    {
                        if (SyncManager.TryGetSyncable(data.syncId, out var destructable) && destructable is PropSyncable destructableSyncable && destructableSyncable.TryGetExtender<ObjectDestructableExtender>(out var extender))
                        {
                            var objectDestructable = extender.GetComponent(data.destructableIndex);
                            ObjectDestructablePatches.IgnorePatches = true;
                            AssetPooleePatches.IgnorePatches = true;

                            objectDestructable._hits = objectDestructable.reqHitCount + 1;
                            objectDestructable.TakeDamage(Vector3Extensions.up, objectDestructable._health + 1f, false, AttackType.Blunt);
                            
                            ObjectDestructablePatches.IgnorePatches = false;
                            AssetPooleePatches.IgnorePatches = false;
                        }
                    }
                }
            }
        }
    }
}
