using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Entities;

using MelonLoader;

using System.Collections;

namespace LabFusion.Network;

public class DespawnResponseData : IFusionSerializable
{
    public const int Size = sizeof(ushort) + sizeof(byte) * 2;

    public ushort syncId;
    public byte despawnerId;

    public void Serialize(FusionWriter writer)
    {
        writer.Write(syncId);
        writer.Write(despawnerId);
    }

    public void Deserialize(FusionReader reader)
    {
        syncId = reader.ReadUInt16();
        despawnerId = reader.ReadByte();
    }

    public static DespawnResponseData Create(ushort syncId, byte despawnerId)
    {
        return new DespawnResponseData()
        {
            syncId = syncId,
            despawnerId = despawnerId,
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

        MelonCoroutines.Start(Internal_WaitForValidDespawn(data.syncId, data.despawnerId));
    }

    private static IEnumerator Internal_WaitForValidDespawn(ushort syncId, byte despawnerId)
    {
        // Delay at most 300 frames until this entity exists
        int i = 0;
        while (!NetworkEntityManager.IdManager.RegisteredEntities.HasEntity(syncId))
        {
            yield return null;

            i++;

            if (i >= 300)
            {
                yield break;
            }
        }

        // Get the entity from the valid id
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(syncId);

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

        poolee.Despawn();

        NetworkEntityManager.IdManager.UnregisterEntity(entity);

        PooleeUtilities.CanDespawn = false;
    }
}