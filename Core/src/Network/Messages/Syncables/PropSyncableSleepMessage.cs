using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Syncables;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using LabFusion.Extensions;

namespace LabFusion.Network
{
    public class PropSyncableSleepData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + sizeof(ushort);

        public byte ownerId;
        public ushort syncId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(ownerId);
            writer.Write(syncId);
        }

        public void Deserialize(FusionReader reader)
        {
            ownerId = reader.ReadByte();
            syncId = reader.ReadUInt16();
        }

        public PropSyncable GetPropSyncable() {
            if (SyncManager.TryGetSyncable(syncId, out var syncable) && syncable is PropSyncable propSyncable)
                return propSyncable;

            return null;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static PropSyncableSleepData Create(byte ownerId, ushort syncId)
        {
            return new PropSyncableSleepData {
                ownerId = ownerId,
                syncId = syncId,
            };
        }
    }

    [Net.SkipHandleWhileLoading]
    public class PropSyncableSleepMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.PropSyncableSleep;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<PropSyncableSleepData>()) {
                    // Find the prop syncable and notify it to sleep
                    var syncable = data.GetPropSyncable();
                    if (syncable != null && syncable.IsRegistered() && syncable.Owner.HasValue && syncable.Owner.Value == data.ownerId) {
                        syncable.IsSleeping = true;
                    }

                    // Send message to other clients if server
                    if (NetworkInfo.IsServer && isServerHandled) {
                        using (var message = FusionMessage.Create(Tag.Value, bytes)) {
                            MessageSender.BroadcastMessageExcept(data.ownerId, NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
