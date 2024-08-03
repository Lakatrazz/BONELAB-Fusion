using LabFusion.Data;
using LabFusion.Exceptions;

namespace LabFusion.Network
{
    public class DespawnRequestData : IFusionSerializable
    {
        public const int Size = sizeof(ushort) + sizeof(byte) * 2;

        public ushort syncId;
        public byte despawnerId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(syncId);
            writer.Write(despawnerId);
        }

        public void Deserialize(FusionReader reader)
        {
            syncId = reader.ReadUInt16();
            despawnerId = reader.ReadByte();
        }

        public static DespawnRequestData Create(ushort syncId, byte despawnerId)
        {
            return new DespawnRequestData()
            {
                syncId = syncId,
                despawnerId = despawnerId,
            };
        }
    }

    [Net.DelayWhileTargetLoading]
    public class DespawnRequestMessage : FusionMessageHandler
    {
        public override byte Tag => NativeMessageTag.DespawnRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            // If we aren't the server, throw an error
            if (!isServerHandled)
            {
                throw new ExpectedServerException();
            }

            using var reader = FusionReader.Create(bytes);
            var readData = reader.ReadFusionSerializable<DespawnRequestData>();

            using var writer = FusionWriter.Create(DespawnResponseData.Size);
            var data = DespawnResponseData.Create(readData.syncId, readData.despawnerId);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.DespawnResponse, writer);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
        }
    }
}
