using LabFusion.Network;
using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Entities;
using LabFusion.Player;

using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Senders;

public static class SpawnSender
{
    /// <summary>
    /// Sends a catchup for the OnPlaceEvent for a CrateSpawner.
    /// </summary>
    /// <param name="placer"></param>
    /// <param name="syncable"></param>
    /// <param name="userId"></param>
    public static void SendCratePlacerCatchup(CrateSpawner placer, NetworkEntity entity, PlayerId player)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        var data = CrateSpawnerData.Create(entity.Id, placer.gameObject);

        MessageRelay.RelayNative(data, NativeMessageTag.CrateSpawner, NetworkChannel.Reliable, RelayType.ToTarget, player.SmallId);
    }

    /// <summary>
    /// Sends the OnPlaceEvent for a CrateSpawner.
    /// </summary>
    /// <param name="placer"></param>
    /// <param name="go"></param>
    public static void SendCratePlacerEvent(CrateSpawner placer, ushort spawnedId)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        // Wait for the level to load and for 5 frames before sending messages
        FusionSceneManager.HookOnLevelLoad(() =>
        {
            DelayUtilities.Delay(() =>
            {
                Internal_OnSendCratePlacer(placer, spawnedId);
            }, 5);
        });
    }

    private static void Internal_OnSendCratePlacer(CrateSpawner placer, ushort spawnedId)
    {
        var data = CrateSpawnerData.Create(spawnedId, placer.gameObject);

        MessageRelay.RelayNative(data, NativeMessageTag.CrateSpawner, NetworkChannel.Reliable, RelayType.ToOtherClients);

        // Insert the catchup hook for future users
        var entity = NetworkEntityManager.IdManager.RegisteredEntities.GetEntity(spawnedId);

        if (entity != null)
        {
            entity.OnEntityCatchup += (entity, player) =>
            {
                SendCratePlacerCatchup(placer, entity, player);
            };
        }
    }

    /// <summary>
    /// Sends a catchup sync message for a pool spawned object.
    /// </summary>
    /// <param name="syncable"></param>
    public static void SendCatchupSpawn(byte owner, string barcode, ushort syncId, SerializedTransform serializedTransform, byte playerId)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        var data = SpawnResponseData.Create(owner, barcode, syncId, serializedTransform);

        MessageRelay.RelayNative(data, NativeMessageTag.SpawnResponse, NetworkChannel.Reliable, RelayType.ToTarget, playerId);
    }
}