using LabFusion.Utilities;
using LabFusion.Patching;
using LabFusion.Entities;

using UnityEngine;

using Il2CppSLZ.Marrow.Warehouse;

using System.Collections;

using MelonLoader;

using LabFusion.Network.Serialization;

namespace LabFusion.Network;

public class CrateSpawnerData : INetSerializable
{
    public const int Size = sizeof(ushort);

    public ushort spawnedId;
    public GameObject placer;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref spawnedId);
        serializer.SerializeValue(ref placer);
    }

    public static CrateSpawnerData Create(ushort spawnedId, GameObject placer)
    {
        return new CrateSpawnerData()
        {
            spawnedId = spawnedId,
            placer = placer,
        };
    }
}

[Net.DelayWhileTargetLoading]
public class CrateSpawnerMessage : NativeMessageHandler
{
    public override byte Tag => NativeMessageTag.CrateSpawner;

    public override ExpectedReceiverType ExpectedReceiver => ExpectedReceiverType.ClientsOnly;

    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<CrateSpawnerData>();

        if (data.placer != null)
        {
            MelonCoroutines.Start(Internal_WaitForSyncable(data.placer, data.spawnedId));
        }
    }

    private static IEnumerator Internal_WaitForSyncable(GameObject placer, ushort spawnId)
    {
        float startTime = TimeUtilities.TimeSinceStartup;

        NetworkEntity entity = null;

        while (entity == null && TimeUtilities.TimeSinceStartup - startTime <= 1f)
        {
            yield return null;

            entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(spawnId);
        }

        if (entity == null)
        {
            yield break;
        }

        var pooleeExtender = entity.GetExtender<PooleeExtender>();

        if (pooleeExtender == null)
        {
            yield break;
        }

        var crateSpawner = placer.GetComponentInChildren<CrateSpawner>(true);

        if (crateSpawner)
        {
            crateSpawner.OnFinishNetworkSpawn(pooleeExtender.Component.gameObject);
        }
    }
}