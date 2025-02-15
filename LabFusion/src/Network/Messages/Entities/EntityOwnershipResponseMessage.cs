using LabFusion.Entities;
using LabFusion.Player;

namespace LabFusion.Network;

public class EntityOwnershipResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityOwnershipResponse;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<EntityPlayerData>();

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.entityId);

        entity?.SetOwner(PlayerIdManager.GetPlayerId(data.playerId));
    }
}