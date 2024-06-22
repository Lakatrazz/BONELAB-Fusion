using LabFusion.Data;
using LabFusion.Syncables;
using LabFusion.Senders;
using LabFusion.Entities;
using LabFusion.Representation;
using LabFusion.Marrow;

namespace LabFusion.Network;

public class NetworkPropCreateData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort);

    public byte ownerId;
    public MarrowEntityHelper.EntityLookupData lookupData;
    public ushort entityId;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(ownerId);
        writer.Write(lookupData);
        writer.Write(entityId);
    }

    public void Deserialize(FusionReader reader)
    {
        ownerId = reader.ReadByte();
        lookupData = reader.ReadFusionSerializable<MarrowEntityHelper.EntityLookupData>();
        entityId = reader.ReadUInt16();
    }

    public static NetworkPropCreateData Create(byte ownerId, MarrowEntityHelper.EntityLookupData lookupData, ushort entityId)
    {
        return new NetworkPropCreateData()
        {
            ownerId = ownerId,
            lookupData = lookupData,
            entityId = entityId,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class NetworkPropCreateMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.NetworkPropCreate;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using FusionReader reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<NetworkPropCreateData>();

        var marrowEntity = MarrowEntityHelper.GetEntityFromLookup(data.lookupData);

        // If this is handled by the server, broadcast the prop creation
        if (isServerHandled)
        {
            if (marrowEntity != null && !marrowEntity.gameObject.IsSyncWhitelisted())
            {
                return;
            }

            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.ownerId, NetworkChannel.Reliable, message, false);
            return;
        }

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
        networkEntity.SetOwner(PlayerIdManager.GetPlayerId(data.ownerId));

        // Insert catchup hook for future users
        networkEntity.OnEntityCatchup += (entity, player) =>
        {
            PropSender.SendCatchupCreation(networkProp, player);
        };
    }
}