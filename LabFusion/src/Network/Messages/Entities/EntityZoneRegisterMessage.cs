using System.Collections;

using Il2CppSLZ.Marrow.Interaction;

using LabFusion.Entities;
using LabFusion.Marrow.Zones;
using LabFusion.Network.Serialization;

using MelonLoader;

namespace LabFusion.Network;

public class EntityZoneRegisterData : INetSerializable
{
    public const int Size = sizeof(byte) + sizeof(ushort) + sizeof(int);

    public ushort entityId;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref entityId);
    }

    public static EntityZoneRegisterData Create(ushort entityId)
    {
        return new EntityZoneRegisterData()
        {
            entityId = entityId,
        };
    }
}

public class EntityZoneRegisterMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.EntityZoneRegister;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<EntityZoneRegisterData>();

        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(data.entityId);

        if (entity == null)
        {
            return;
        }

        if (received.Sender != entity.OwnerId.SmallId)
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