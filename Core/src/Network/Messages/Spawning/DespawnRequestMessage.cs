using LabFusion.Data;
using LabFusion.Patching;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;

using System;

namespace LabFusion.Network
{
    public class DespawnRequestData : IFusionSerializable, IDisposable
    {
        public ushort syncId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(syncId);
        }

        public void Deserialize(FusionReader reader)
        {
            syncId = reader.ReadUInt16();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static DespawnRequestData Create(ushort syncId)
        {
            return new DespawnRequestData()
            {
                syncId = syncId,
            };
        }
    }

    [Net.DelayWhileLoading]
    public class DespawnRequestMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.DespawnRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            // If this is the server, send this to clients
            if (isServerHandled) {
                using (var reader = FusionReader.Create(bytes)) {
                    using (var readData = reader.ReadFusionSerializable<DespawnRequestData>()) {
                        using (var writer = FusionWriter.Create()) {
                            using (var data = DespawnResponseData.Create(readData.syncId)) {
                                writer.Write(data);

                                using (var message = FusionMessage.Create(NativeMessageTag.DespawnResponse, writer)) {
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
