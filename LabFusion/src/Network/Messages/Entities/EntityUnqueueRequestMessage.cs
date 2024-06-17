using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Exceptions;

namespace LabFusion.Network
{
    public class EntityUnqueueRequestData : IFusionSerializable
    {
        public const int Size = sizeof(byte) + sizeof(ushort);

        public byte userId;
        public ushort queuedId;

        public void Serialize(FusionWriter writer)
        {
            writer.Write(userId);
            writer.Write(queuedId);
        }

        public void Deserialize(FusionReader reader)
        {
            userId = reader.ReadByte();
            queuedId = reader.ReadUInt16();
        }

        public static EntityUnqueueRequestData Create(byte userId, ushort queuedId)
        {
            return new EntityUnqueueRequestData()
            {
                userId = userId,
                queuedId = queuedId
            };
        }
    }

    public class EntityUnqueueRequestMessage : FusionMessageHandler
    {
        public override byte? Tag => NativeMessageTag.EntityUnqueueRequest;

        public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
        {
            if (!isServerHandled)
            {
                throw new ExpectedServerException();
            }

            using var reader = FusionReader.Create(bytes);
            var data = reader.ReadFusionSerializable<EntityUnqueueRequestData>();

            using var writer = FusionWriter.Create(EntityUnqueueResponseData.Size);

            var allocatedId = NetworkEntityManager.IdManager.RegisteredEntities.AllocateNewId();
            var response = EntityUnqueueResponseData.Create(data.queuedId, allocatedId);

            writer.Write(response);

            using var message = FusionMessage.Create(NativeMessageTag.EntityUnqueueResponse, writer);
            MessageSender.SendFromServer(data.userId, NetworkChannel.Reliable, message);
        }
    }
}
