using LabFusion.Data;
using LabFusion.Senders;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Marrow;
using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class NetworkPropCreateData : INetSerializable
{
    public const int Size = sizeof(byte) + ComponentHashData.Size + sizeof(ushort);

    public byte OwnerID;
    public ComponentHashData HashData;
    public ushort EntityID;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref OwnerID);
        serializer.SerializeValue(ref HashData);
        serializer.SerializeValue(ref EntityID);
    }

    public static NetworkPropCreateData Create(byte ownerID, ComponentHashData hashData, ushort entityID)
    {
        return new NetworkPropCreateData()
        {
            OwnerID = ownerID,
            HashData = hashData,
            EntityID = entityID,
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

        var marrowEntity = MarrowEntityHelper.GetEntityFromData(data.HashData);

        // Make sure the marrow entity exists
        if (marrowEntity == null)
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
        NetworkEntityManager.IDManager.RegisterEntity(data.EntityID, networkEntity);

        // Set the owner to the received owner id
        var ownerId = PlayerIDManager.GetPlayerID(data.OwnerID);

        networkEntity.SetOwner(ownerId);

        // Insert creation catchup hook for future users
        networkEntity.OnEntityCreationCatchup += (entity, player) =>
        {
            PropSender.SendCatchupCreation(networkProp, player);
        };

        CatchupManager.RequestEntityDataCatchup(new(networkEntity));
    }
}