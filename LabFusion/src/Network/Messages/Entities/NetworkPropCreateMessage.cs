using LabFusion.Data;
using LabFusion.Syncables;
using LabFusion.Senders;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Marrow;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class NetworkPropCreateData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte ownerId;
    public ComponentHashData hashData;
    public ushort entityId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref ownerId);
        serializer.SerializeValue(ref hashData);
        serializer.SerializeValue(ref entityId);
    }

    public static NetworkPropCreateData Create(byte ownerId, ComponentHashData hashData, ushort entityId)
    {
        return new NetworkPropCreateData()
        {
            ownerId = ownerId,
            hashData = hashData,
            entityId = entityId,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class NetworkPropCreateMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.NetworkPropCreate;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<NetworkPropCreateData>();

        var marrowEntity = MarrowEntityHelper.GetEntityFromData(data.hashData);

        // Make sure the marrow entity exists
        if (marrowEntity == null)
        {
            return;
        }

        // Check if its blacklisted
        if (!marrowEntity.gameObject.IsSyncWhitelisted())
        {
            return;
        }

        // Check if it already has an entity attached
        if (IMarrowEntityExtender.Cache.ContainsSource(marrowEntity))
        {
            return;
        }

        // Create a new network entity and network prop
        NetworkEntity networkEntity = new();
        NetworkProp networkProp = new(networkEntity, marrowEntity);

        // Register the entity with the sent id
        NetworkEntityManager.IdManager.RegisterEntity(data.entityId, networkEntity);

        // Set the owner to the received owner id
        var ownerId = PlayerIdManager.GetPlayerId(data.ownerId);

        networkEntity.SetOwner(ownerId);

        // Insert creation catchup hook for future users
        networkEntity.OnEntityCreationCatchup += (entity, player) =>
        {
            PropSender.SendCatchupCreation(networkProp, player);
        };

        CatchupManager.RequestEntityDataCatchup(new(networkEntity));
    }
}