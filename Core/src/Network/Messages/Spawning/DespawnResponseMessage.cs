using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;

using System;

namespace LabFusion.Network
{
    public class DespawnResponseData : IFusionSerializable, IDisposable
    {
        public ushort syncId;

        public void Serialize(FusionWriter writer) {
            writer.Write(syncId);
        }

        public void Deserialize(FusionReader reader) {
            syncId = reader.ReadUInt16();
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
        }

        public static DespawnResponseData Create(ushort syncId)
        {
            return new DespawnResponseData()
            {
                syncId = syncId,
            };
        }
    }

    public class DespawnResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.DespawnResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            // Despawn the poolee if it exists
            using (var reader = FusionReader.Create(bytes)) {
                using (var data = reader.ReadFusionSerializable<DespawnResponseData>()) {
                    if (SyncManager.TryGetSyncable(data.syncId, out var syncable) && syncable is PropSyncable propSyncable) {
                        PooleeUtilities.CanDespawn = true;

                        if (propSyncable.AssetPoolee && propSyncable.AssetPoolee.gameObject.activeInHierarchy)
                            propSyncable.AssetPoolee.Despawn();

                        PooleeUtilities.CanDespawn = false;
                    }
                }
            }
        }
    }
}
