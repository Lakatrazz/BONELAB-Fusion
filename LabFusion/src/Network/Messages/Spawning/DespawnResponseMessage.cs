using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Entities;

using MelonLoader;

using System.Collections;
using Il2CppSLZ.Marrow.VFX;

namespace LabFusion.Network;

public class DespawnResponseData : IFusionSerializable
{
    public const int Size = sizeof(ushort) + sizeof(byte) * 2;

    public byte despawnerId;
    public ushort entityId;

    public bool despawnEffect;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(despawnerId);
        writer.Write(entityId);

        writer.Write(despawnEffect);
    }

    public void Deserialize(FusionReader reader)
    {
        despawnerId = reader.ReadByte();
        entityId = reader.ReadUInt16();

        despawnEffect = reader.ReadBoolean();
    }

    public static DespawnResponseData Create(byte despawnerId, ushort entityId, bool despawnEffect)
    {
        return new DespawnResponseData()
        {
            despawnerId = despawnerId,
            entityId = entityId,

            despawnEffect = despawnEffect,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class DespawnResponseMessage : FusionMessageHandler
{
    public override byte Tag => NativeMessageTag.DespawnResponse;

    public override void HandleMessage(byte[] bytes, bool isServerHandled = false)
    {
        // Despawn the poolee if it exists
        using var reader = FusionReader.Create(bytes);
        var data = reader.ReadFusionSerializable<DespawnResponseData>();

        MelonCoroutines.Start(Internal_WaitForValidDespawn(data.despawnerId, data.entityId, data.despawnEffect));
    }

    private static IEnumerator Internal_WaitForValidDespawn(byte despawnerId, ushort entityId, bool despawnEffect)
    {
        // Delay at most 300 frames until this entity exists
        int i = 0;
        while (!NetworkEntityManager.IdManager.RegisteredEntities.HasEntity(entityId))
        {
            yield return null;

            i++;

            if (i >= 300)
            {
                yield break;
            }
        }

        // Get the entity from the valid id
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(entityId);

        if (entity == null)
        {
            yield break;
        }

        var pooleeExtender = entity.GetExtender<PooleeExtender>();

        if (pooleeExtender == null)
        {
            yield break;
        }

        PooleeUtilities.CanDespawn = true;

        var poolee = pooleeExtender.Component;

#if DEBUG
        FusionLogger.Log($"Unregistering entity at ID {entity.Id} after despawning.");
#endif

        var marrowEntity = entity.GetExtender<IMarrowEntityExtender>();

        if (marrowEntity != null && despawnEffect)
        {
            SpawnEffects.CallDespawnEffect(marrowEntity.MarrowEntity);
        }

        poolee.Despawn();

        NetworkEntityManager.IdManager.UnregisterEntity(entity);

        PooleeUtilities.CanDespawn = false;
    }
}