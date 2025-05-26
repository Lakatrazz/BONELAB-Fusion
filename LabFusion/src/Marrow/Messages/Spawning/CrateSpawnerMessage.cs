using LabFusion.Marrow.Patching;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.Data;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Player;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow.Messages;

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

[Net.SkipHandleWhileLoading]
public class CrateSpawnerMessage : ModuleMessageHandler
{
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

            entity.OnEntityDataCatchup += (entity, player) =>
            {
                SendCrateSpawnerMessage(crateSpawner, entity.ID, player);
            };
        }
    }

    public static void SendCrateSpawnerMessage(CrateSpawner crateSpawner, ushort entityID, PlayerID target = null)
    {
        var hashData = CrateSpawnerPatches.HashTable.GetDataFromComponent(crateSpawner);

        var data = CrateSpawnerData.Create(entityID, hashData);

        if (target != null)
        {
            MessageRelay.RelayModule<CrateSpawnerMessage, CrateSpawnerData>(data, NetworkChannel.Reliable, RelayType.ToTarget, target.SmallID);
        }
        else
        {
            MessageRelay.RelayModule<CrateSpawnerMessage, CrateSpawnerData>(data, NetworkChannel.Reliable, RelayType.ToOtherClients);
        }
    }
}