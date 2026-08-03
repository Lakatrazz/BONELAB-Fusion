using LabFusion.Entities;

namespace LabFusion.Network;

public class EntityOwnershipRequestMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityOwnershipRequest;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ServerOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        // Read request
        var data = received.ReadData<EntityPlayerData>();

        if (data.Entity.TryGetEntity(out var networkEntity) && networkEntity.HasLinkedEntities)
        {
            PropagateOwnership(data.PlayerID, networkEntity);
        }
        else
        {
            SendOwnershipResponse(data.PlayerID, data.Entity.ID);
        }
    }

    private static void PropagateOwnership(ClientSmallID ownerID, NetworkEntity networkEntity)
    {
        var ownableEntities = EntityGraphTraversal.GetAllOwnableLinkedEntities(networkEntity, out var lockedOwner);

        if (lockedOwner.HasValue && lockedOwner != ownerID)
        {
            return;
        }

        foreach (var entity in ownableEntities)
        {
            SendOwnershipResponse(ownerID, entity.ID);
        }
    }

    private static void SendOwnershipResponse(ClientSmallID ownerID, ushort entityID)
    {
        var response = new EntityPlayerData()
        {
            PlayerID = ownerID,
            Entity = new(entityID),
        };

        MessageRelay.RelayNative(response, NativeMessageTag.EntityOwnershipResponse, CommonMessageRoutes.ReliableToClients);
    }
}