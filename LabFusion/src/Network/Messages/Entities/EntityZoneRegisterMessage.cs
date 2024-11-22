using System.Collections;

using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.Marrow.Zones;

using MelonLoader;

namespace LabFusion.Network;

public class EntityZoneRegisterData : IFusionSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort) + sizeof(int);

    public byte ownerId;
    public ushort entityId;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(ownerId);
        writer.Write(entityId);
    }

    public void Deserialize(FusionReader reader)
    {
        ownerId = reader.ReadByte();
        entityId = reader.ReadUInt16();
    }

    public static EntityZoneRegisterData Create(byte ownerId, ushort entityId)
    {
        return new EntityZoneRegisterData()
        {
            ownerId = ownerId,
            entityId = entityId,
        };
    }
}

public class EntityZoneRegisterMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityZoneRegister;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<EntityZoneRegisterData>();

        // Send message to other clients if server
        if (isServerHandled)
        {
            using var message = FusionMessage.Create(Tag, bytes);
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

        // Unculling will cause the NetworkEntity to teleport to its pose
        // Sometimes it can get immediately culled right after, so do it for a few frames
        // Since its actually moving locations, this should make the zones track the entity properly
        MelonCoroutines.Start(RepeatCull(marrowExtender.MarrowEntity));
    }

    private static IEnumerator RepeatCull(MarrowEntity entity)
    {
        for (var i = 0; i < 3; i++)
        {
            SafeZoneCuller.Cull(entity, false);

            yield return null;
        }
    }
}