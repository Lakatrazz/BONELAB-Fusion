using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Syncables;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public class SyncableOwnershipResponseData : IFusionSerializable, IDisposable
    {
        public const int Size = sizeof(byte) + sizeof(ushort);

        public byte smallId;
        public ushort syncId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(smallId);
            writer.Write(syncId);
        }

        public void Deserialize(FusionReader reader)
        {
            smallId = reader.ReadByte();
            syncId = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static SyncableOwnershipResponseData Create(byte smallId, ushort syncId)
        {
            return new SyncableOwnershipResponseData()
            {
                smallId = smallId,
                syncId = syncId
            };
        }
    }

    public class SyncableOwnershipResponseMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SyncableOwnershipResponse;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (!isServerHandled) {
                using (var reader = FusionReader.Create(bytes)) {
                    using (var data = reader.ReadFusionSerializable<SyncableOwnershipResponseData>()) {
                        if (SyncManager.TryGetSyncable(data.syncId, out var syncable)) {
                            syncable.SetOwner(data.smallId);
                        }
                    }
                }
            }
        }
    }
}
