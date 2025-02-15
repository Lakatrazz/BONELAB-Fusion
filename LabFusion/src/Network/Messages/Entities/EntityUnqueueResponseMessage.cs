using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Exceptions;
using LabFusion.Utilities;

namespace LabFusion.Network;

public class EntityUnqueueResponseData : IFusionSerializable
{
    public const int Size = sizeof(ushort) * 2;

    public ushort queuedId;
    public ushort allocatedId;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(queuedId);
        writer.Write(allocatedId);
    }

    public void Deserialize(FusionReader reader)
    {
        queuedId = reader.ReadUInt16();
        allocatedId = reader.ReadUInt16();
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

        var (success, entity) = NetworkEntityManager.IdManager.UnqueueEntity(data.queuedId, data.allocatedId);

#if DEBUG
        if (success)
        {
            FusionLogger.Log($"Unqueued Entity with allocated id {entity.Id}.");
        }
#endif
    }
}