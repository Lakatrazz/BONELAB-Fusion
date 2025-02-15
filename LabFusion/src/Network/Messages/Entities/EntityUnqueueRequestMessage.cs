using LabFusion.Data;
using LabFusion.Entities;

namespace LabFusion.Network;

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

public class EntityUnqueueRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityUnqueueRequest;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<EntityUnqueueRequestData>();

        var allocatedId = NetworkEntityManager.IdManager.RegisteredEntities.AllocateNewId();

        var response = EntityUnqueueResponseData.Create(data.queuedId, allocatedId);

        MessageRelay.RelayNative(response, NativeMessageTag.EntityUnqueueResponse, NetworkChannel.Reliable, RelayType.ToTarget, data.userId);
    }
}