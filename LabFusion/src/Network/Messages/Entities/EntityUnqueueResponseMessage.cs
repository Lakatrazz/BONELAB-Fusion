using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class EntityUnqueueResponseData : INetSerializable
{
    public const int Size = sizeof(ushort) * 2;

    public ushort queuedId;
    public ushort allocatedId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref queuedId);
        serializer.SerializeValue(ref allocatedId);
    }

    public static EntityUnqueueResponseData Create(ushort queuedId, ushort allocatedId)
    {
        return new EntityUnqueueResponseData()
        {
            queuedId = queuedId,
            allocatedId = allocatedId
        };
    }
}

public class EntityUnqueueResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityUnqueueResponse;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<EntityUnqueueResponseData>();

        var (success, entity) = NetworkEntityManager.IDManager.UnqueueEntity(data.queuedId, data.allocatedId);

#if DEBUG
        if (success)
        {
            FusionLogger.Log($"Unqueued Entity with allocated id {entity.ID}.");
        }
#endif
    }
}