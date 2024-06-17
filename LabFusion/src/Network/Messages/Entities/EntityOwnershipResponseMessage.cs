using LabFusion.Entities;
using LabFusion.Exceptions;
using LabFusion.Representation;

namespace LabFusion.Network;

public class EntityOwnershipResponseMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.EntityOwnershipResponse;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        if (isServerHandled)
        {
            throw new ExpectedClientException();
        }

        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<EntityPlayerData>();

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.entityId);

        entity?.SetOwner(PlayerIdManager.GetPlayerId(data.playerId));
    }
}