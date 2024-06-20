using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Zones;
using LabFusion.Patching;

namespace LabFusion.Network;

public class EntityZoneRegisterData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort) + sizeof(int);

    public byte ownerId;
    public ushort entityId;
    public int zoneHash;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(ownerId);
        writer.Write(entityId);
        writer.Write(zoneHash);
    }

    public void Deserialize(FusionReader reader)
    {
        ownerId = reader.ReadByte();
        entityId = reader.ReadUInt16();
        zoneHash = reader.ReadInt32();
    }

    public static EntityZoneRegisterData Create(byte ownerId, ushort entityId, int zoneHash)
    {
        return new EntityZoneRegisterData()
        {
            ownerId = ownerId,
            entityId = entityId,
            zoneHash = zoneHash
        };
    }
}

public class EntityZoneRegisterMessage : FusionMessageHandler
{
    public override byte? Tag => NativeMessageTag.EntityZoneRegister;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<EntityZoneRegisterData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag.Value, bytes);
            MessageSender.BroadcastMessageExcept(data.ownerId, NetworkChannel.Reliable, message, false);
            return;
        }

        // Get entity
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.entityId);

        if (entity == null)
        {
            return;
        }

        var marrowExtender = entity.GetExtender<IMarrowEntityExtender>();

        if (marrowExtender == null)
        {
            return;
        }

        if (marrowExtender.MarrowEntity == null)
        {
            return;
        }

        // Find zone
        var hash = data.zoneHash;
        
        if (!ZoneCullerPatches.HashToZone.TryGetValue(hash, out var culler))
        {
            return;
        }

        var cullerId = culler._zoneId;

        // Migrate entity
        ZoneCullHelper.MigrateEntity(cullerId, marrowExtender.MarrowEntity);
    }
}