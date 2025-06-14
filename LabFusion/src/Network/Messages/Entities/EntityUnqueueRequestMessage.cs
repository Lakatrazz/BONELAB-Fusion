using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Player;

namespace LabFusion.Network;

public class EntityUnqueueRequestData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte userId;
    public ushort queuedId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref userId);
        serializer.SerializeValue(ref queuedId);
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

        var allocatedId = NetworkEntityManager.IDManager.RegisteredEntities.AllocateNewID();

        var response = EntityUnqueueResponseData.Create(data.queuedId, allocatedId);

        MessageRelay.RelayNative(response, NativeMessageTag.EntityUnqueueResponse, new MessageRoute(data.userId, NetworkChannel.Reliable));
    }
}