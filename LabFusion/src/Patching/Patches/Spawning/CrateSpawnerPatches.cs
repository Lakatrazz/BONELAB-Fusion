using HarmonyLib;

using LabFusion.RPC;
using LabFusion.Senders;
using LabFusion.Scene;
using LabFusion.Marrow.Integration;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Pool;

using UnityEngine;

using Il2CppCysharp.Threading.Tasks;

namespace LabFusion.Patching;

// SpawnSpawnableAsync is used by the regular SpawnSpawnable as well, so we don't need to patch that
[HarmonyPatch(typeof(CrateSpawner))]
public static class CrateSpawnerPatches
{
    public static readonly HashSet<CrateSpawner> CurrentlySpawning = new();

    private static void NetworkedSpawnSpawnable(CrateSpawner spawner, UniTaskCompletionSource<Poolee> source)
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
                OnNetworkSpawn(spawner, info, source);
            },
        });

        CurrentlySpawning.Add(spawner);
    }

    private static void OnNetworkSpawn(CrateSpawner spawner, NetworkAssetSpawner.SpawnCallbackInfo info, UniTaskCompletionSource<Poolee> source)
    {
        var spawned = info.spawned;
        spawner.OnFinishNetworkSpawn(spawned);

        var poolee = Poolee.Cache.Get(spawned);

        source.TrySetResult(poolee);

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
    [HarmonyPatch(nameof(CrateSpawner.SpawnSpawnableAsync))]
    public static bool SpawnSpawnableAsyncPrefix(CrateSpawner __instance, bool isHidden, ref UniTask<Poolee> __result)
    {
        // If this scene is unsynced, the spawner can function as normal.
        if (CrossSceneManager.InUnsyncedScene())
        {
            return true;
        }

        var spawner = __instance;

        // Check if this CrateSpawner has a Desyncer
        if (Desyncer.Cache.ContainsSource(spawner.gameObject))
        {
            return true;
        }

        // If we aren't the scene host, don't allow a crate spawn
        if (!CrossSceneManager.IsSceneHost())
        {
            __result = new UniTask<Poolee>(null);
            return false;
        }

        // Make sure this isn't already spawning
        if (CurrentlySpawning.Any((found) => found == spawner))
        {
            __result = new UniTask<Poolee>(null);
            return false;
        }
        
        var source = new UniTaskCompletionSource<Poolee>();
        __result = new UniTask<Poolee>(source.TryCast<IUniTaskSource<Poolee>>(), default);

        // Otherwise, manually sync this spawn over the network
        NetworkedSpawnSpawnable(spawner, source);

        return false;
    }
}