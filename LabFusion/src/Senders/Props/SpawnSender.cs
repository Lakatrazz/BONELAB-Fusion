using LabFusion.Network;
using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Entities;
using LabFusion.Player;
using LabFusion.Patching;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Senders;

public static class SpawnSender
{
    /// <summary>
    /// Sends a catchup for the OnPlaceEvent for a CrateSpawner.
    /// </summary>
    /// <param name="spawner"></param>
    /// <param name="entity"></param>
    /// <param name="player"></param>
    public static void SendCrateSpawnerCatchup(CrateSpawner spawner, NetworkEntity entity, PlayerId player)
    {
        var hashData = CrateSpawnerPatches.HashTable.GetDataFromComponent(spawner);

        var data = CrateSpawnerData.Create(entity.Id, hashData);

        MessageRelay.RelayNative(data, NativeMessageTag.CrateSpawner, NetworkChannel.Reliable, RelayType.ToTarget, player.SmallId);
    }

    /// <summary>
    /// Sends the OnPlaceEvent for a CrateSpawner.
    /// </summary>
    /// <param name="spawner"></param>
    /// <param name="go"></param>
    public static void SendCrateSpawnerEvent(CrateSpawner spawner, ushort spawnedId)
    {
        // Wait for the level to load and for 5 frames before sending messages
        FusionSceneManager.HookOnLevelLoad(() =>
        {
            DelayUtilities.Delay(() =>
            {
                OnSendCrateSpawner(spawner, spawnedId);
            }, 5);
        });
    }

    private static void OnSendCrateSpawner(CrateSpawner spawner, ushort spawnedId)
    {
        var hashData = CrateSpawnerPatches.HashTable.GetDataFromComponent(spawner);

        var data = CrateSpawnerData.Create(spawnedId, hashData);

        MessageRelay.RelayNative(data, NativeMessageTag.CrateSpawner, NetworkChannel.Reliable, RelayType.ToOtherClients);

        // Insert the catchup hook for future users
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(spawnedId);

        if (entity != null)
        {
            entity.OnEntityDataCatchup += (entity, player) =>
            {
                SendCrateSpawnerCatchup(spawner, entity, player);
            };
        }
    }

    /// <summary>
    /// Sends a catchup sync message for a pool spawned object.
    /// </summary>
    /// <param name="syncable"></param>
    public static void SendCatchupSpawn(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, byte playerId)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        var data = SpawnResponseData.Create(owner, barcode, syncId, serializedTransform);

        MessageRelay.RelayNative(data, NativeMessageTag.SpawnResponse, NetworkChannel.Reliable, RelayType.ToTarget, playerId);
    }
}