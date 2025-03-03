using LabFusion.Patching;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Data;

namespace LabFusion.Network;

public class CrateSpawnerData : INetSerializable
{
    public const int Size = sizeof(ushort);

    public ushort SpawnedId;
    public ComponentHashData HashData;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SpawnedId);
        serializer.SerializeValue(ref HashData);
    }

    public static CrateSpawnerData Create(ushort spawnedId, ComponentHashData hashData)
    {
        return new CrateSpawnerData()
        {
            SpawnedId = spawnedId,
            HashData = hashData,
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

        var crateSpawner = CrateSpawnerPatches.HashTable.GetComponentFromData(data.HashData);

        if (crateSpawner == null)
        {
            return;
        }

        NetworkEntityManager.HookEntityRegistered(data.SpawnedId, OnSpawnedRegistered);

        void OnSpawnedRegistered(NetworkEntity entity)
        {
            var pooleeExtender = entity.GetExtender<PooleeExtender>();

            if (pooleeExtender == null)
            {
                return;
            }

            crateSpawner.OnFinishNetworkSpawn(pooleeExtender.Component.gameObject);
        }
    }
}