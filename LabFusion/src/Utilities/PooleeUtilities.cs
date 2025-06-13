using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;

using LabFusion.Entities;

namespace LabFusion.Utilities;

public static class PooleeUtilities
{
    public static void DespawnAll()
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        // Loop through all NetworkProps and despawn them
        var entities = NetworkEntityManager.IDManager.RegisteredEntities.EntityIDLookup.Keys.ToArray();
        foreach (var networkEntity in entities)
        {
            var prop = networkEntity.GetExtender<NetworkProp>();

            if (prop == null)
            {
                continue;
            }

            var poolee = networkEntity.GetExtender<PooleeExtender>();

            if (poolee == null)
            {
                continue;
            }

            poolee.Component.Despawn();
        }
    }

    public static void SendDespawn(ushort entityId, bool despawnEffect)
    {
        // Send response
        if (NetworkInfo.IsHost)
        {
            var data = new DespawnResponseData()
            {
                Despawner = new(PlayerIDManager.LocalSmallID),
                Entity = new(entityId),
                DespawnEffect = despawnEffect,
            };

            MessageRelay.RelayNative(data, NativeMessageTag.DespawnResponse, CommonMessageRoutes.ReliableToOtherClients);
        }
        // Send request
        else
        {
            RequestDespawn(entityId, despawnEffect);
        }
    }

    public static void RequestDespawn(ushort entityId, bool despawnEffect)
    {
        var data = new DespawnRequestData()
        {
            Entity = new NetworkEntityReference(entityId),
            DespawnEffect = despawnEffect,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.DespawnRequest, CommonMessageRoutes.ReliableToServer);
    }

    public static void RequestSpawn(string barcode, SerializedTransform serializedTransform, uint trackerId, bool spawnEffect)
    {
        var data = SpawnRequestData.Create(barcode, serializedTransform, trackerId, spawnEffect);

        MessageRelay.RelayNative(data, NativeMessageTag.SpawnRequest, CommonMessageRoutes.ReliableToServer);
    }

    public static void SendSpawn(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, uint trackerId = 0, bool spawnEffect = false)
    {
        var data = SpawnResponseData.Create(owner, barcode, syncId, serializedTransform, trackerId, spawnEffect);

        MessageRelay.RelayNative(data, NativeMessageTag.SpawnResponse, CommonMessageRoutes.ReliableToClients);
    }
}