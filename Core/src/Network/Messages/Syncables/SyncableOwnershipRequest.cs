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
    public class SyncableOwnershipRequestData : IFusionSerializable, IDisposable
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

        public static SyncableOwnershipRequestData Create(byte smallId, ushort syncId)
        {
            return new SyncableOwnershipRequestData()
            {
                smallId = smallId,
                syncId = syncId
            };
        }
    }

    public class SyncableOwnershipRequestMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.SyncableOwnershipRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (NetworkInfo.IsServer && isServerHandled) {
                using (var reader = FusionReader.Create(bytes)) {
                    using (var data = reader.ReadFusionSerializable<SyncableOwnershipRequestData>()) {
                        using (var writer = FusionWriter.Create(SyncableOwnershipResponseData.Size)) {
                            using (var response = SyncableOwnershipResponseData.Create(data.smallId, data.syncId)) {
                                writer.Write(response);

                                using (var message = FusionMessage.Create(NativeMessageTag.SyncableOwnershipResponse, writer)) {
                                    MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
