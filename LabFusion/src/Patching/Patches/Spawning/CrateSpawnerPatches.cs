using HarmonyLib;

using LabFusion.RPC;
using LabFusion.Senders;
using LabFusion.Scene;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Pool;

using UnityEngine;

namespace LabFusion.Patching;

// SpawnSpawnableAsync is used by the regular SpawnSpawnable as well, so we don't need to patch that
[HarmonyPatch(typeof(CrateSpawner._SpawnSpawnableAsync_d__26))]
public static class CrateSpawnerAsyncPatches
{
    public static readonly HashSet<CrateSpawner> CurrentlySpawning = new();

    private static void NetworkedSpawnSpawnable(CrateSpawner spawner)
    {
        var spawnable = spawner._spawnable;
        var transform = spawner.transform;

        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
        {
            spawnable = spawnable,
            position = transform.position,
            rotation = transform.rotation,
            spawnCallback = (info) =>
            {
                OnNetworkSpawn(spawner, info);
            },
        });

        CurrentlySpawning.Add(spawner);
    }

    private static void OnNetworkSpawn(CrateSpawner spawner, NetworkAssetSpawner.SpawnCallbackInfo info)
    {
        var spawned = info.spawned;
        spawner.OnFinishNetworkSpawn(spawned);

        // Make sure we actually have a network entity
        if (info.entity == null)
        {
            return;
        }

        // Send spawn message
        var spawnedId = info.entity.Id;

        SpawnSender.SendCratePlacerEvent(spawner, spawnedId);
    }

    public static void OnFinishNetworkSpawn(this CrateSpawner spawner, GameObject go)
    {
        // Remove from global spawning check
        CurrentlySpawning.RemoveWhere((found) => found == spawner);

        // Invoke spawn events
        spawner.onSpawnEvent?.Invoke(spawner, go);

        var poolee = Poolee.Cache.Get(go);

        spawner.OnPooleeSpawn(go);

        poolee.OnDespawnDelegate += (Action<GameObject>)spawner.OnPooleeDespawn;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CrateSpawner._SpawnSpawnableAsync_d__26.MoveNext))]
    public static bool MoveNext(CrateSpawner._SpawnSpawnableAsync_d__26 __instance)
    {
        // If this scene is unsynced, the spawner can function as normal.
        if (CrossSceneManager.InUnsyncedScene())
        {
            return true;
        }

        // If we aren't the scene host, don't allow a crate spawn
        if (!CrossSceneManager.IsSceneHost())
        {
            return false;
        }

        var spawner = __instance.__4__this;

        // Make sure this isn't already spawning
        if (CurrentlySpawning.Any((found) => found == spawner))
        {
            return false;
        }

        // Otherwise, manually sync this spawn over the network
        NetworkedSpawnSpawnable(spawner);

        return false;
    }
}