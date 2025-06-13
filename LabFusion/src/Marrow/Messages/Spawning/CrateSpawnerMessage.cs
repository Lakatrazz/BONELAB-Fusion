using LabFusion.Marrow.Patching;
using LabFusion.Entities;
using LabFusion.Network.Serialization;
using LabFusion.SDK.Modules;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Marrow.Extenders;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Marrow.Messages;

public class CrateSpawnerData : INetSerializable
{
    public const int Size = sizeof(ushort) + ComponentPathData.Size;

    public int? GetSize() => Size;

    public ushort SpawnedID;
    public ComponentPathData PathData;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref SpawnedID);
        serializer.SerializeValue(ref PathData);
    }
}

[Net.SkipHandleWhileLoading]
public class CrateSpawnerMessage : ModuleMessageHandler
{
    protected override void OnHandleMessage(ReceivedMessage received)
    {
        var data = received.ReadData<CrateSpawnerData>();

        if (!data.PathData.TryGetComponent<CrateSpawner, CrateSpawnerExtender>(CrateSpawnerPatches.HashTable, out var crateSpawner))
        {
            return;
        }

        var entity = NetworkEntityManager.IDManager.RegisteredEntities.GetEntity(data.SpawnedID);

        if (entity == null)
        {
            return;
        }

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

    public static void SendCrateSpawnerMessage(CrateSpawner crateSpawner, ushort entityID, PlayerID target = null)
    {
        var data = new CrateSpawnerData()
        {
            SpawnedID = entityID,
            PathData = ComponentPathData.CreateFromComponent<CrateSpawner, CrateSpawnerExtender>(crateSpawner, CrateSpawnerPatches.HashTable, CrateSpawnerExtender.Cache),
        };

        if (target != null)
        {
            MessageRelay.RelayModule<CrateSpawnerMessage, CrateSpawnerData>(data, new MessageRoute(target.SmallID, NetworkChannel.Reliable));
        }
        else
        {
            MessageRelay.RelayModule<CrateSpawnerMessage, CrateSpawnerData>(data, CommonMessageRoutes.ReliableToClients);
        }
    }
}