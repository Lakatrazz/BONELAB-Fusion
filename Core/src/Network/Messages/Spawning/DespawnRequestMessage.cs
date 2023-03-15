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
        public const int Size = sizeof(ushort) + sizeof(byte) * 2;

        public ushort syncId;
        public byte despawnerId;
        public bool isMag;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(syncId);
            writer.Write(despawnerId);
            writer.Write(isMag);
        }

        public void Deserialize(FusionReader reader)
        {
            syncId = reader.ReadUInt16();
            despawnerId = reader.ReadByte();
            isMag = reader.ReadBoolean();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public static DespawnRequestData Create(ushort syncId, byte despawnerId, bool isMag = false)
        {
            return new DespawnRequestData()
            {
                syncId = syncId,
                despawnerId = despawnerId,
                isMag = isMag,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class DespawnRequestMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.DespawnRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false) {
            // If this is the server, send this to clients
            if (isServerHandled) {
                using (var reader = FusionReader.Create(bytes)) {
                    using (var readData = reader.ReadFusionSerializable<DespawnRequestData>()) {
                        using (var writer = FusionWriter.Create(DespawnResponseData.Size)) {
                            using (var data = DespawnResponseData.Create(readData.syncId, readData.despawnerId, readData.isMag)) {
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
