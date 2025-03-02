using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Entities;

using MelonLoader;

using System.Collections;

using Il2CppSLZ.Marrow.VFX;

using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class DespawnResponseData : INetSerializable
{
    public const int Size = sizeof(ushort) + sizeof(byte) * 2;

    public byte despawnerId;
    public ushort entityId;

    public bool despawnEffect;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref despawnerId);
        serializer.SerializeValue(ref entityId);

        serializer.SerializeValue(ref despawnEffect);
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
public class DespawnResponseMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.DespawnResponse;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        // Despawn the poolee if it exists
        var data = received.ReadData<DespawnResponseData>();

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