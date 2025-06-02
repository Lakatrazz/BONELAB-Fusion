using HarmonyLib;

using LabFusion.RPC;
using LabFusion.Scene;
using LabFusion.Marrow.Integration;
using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Marrow.Messages;

using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow.Pool;

using UnityEngine;

using Il2CppCysharp.Threading.Tasks;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(CrateSpawner))]
public static class CrateSpawnerPatches
{
    public static readonly ComponentHashTable<CrateSpawner> HashTable = new();

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CrateSpawner.Awake))]
    public static void Awake(CrateSpawner __instance)
    {
        var hash = GameObjectHasher.GetHierarchyHash(__instance.gameObject);

        var index = HashTable.AddComponent(hash, __instance);

#if DEBUG
        if (index > 0)
        {
            FusionLogger.Log($"CrateSpawner {__instance.name} had a conflicting hash {hash} and has been added at index {index}.");
        }
#endif
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CrateSpawner.OnDestroy))]
    public static void OnDestroy(CrateSpawner __instance)
    {
        HashTable.RemoveComponent(__instance);
    }

    private static void NetworkedSpawnSpawnable(CrateSpawner spawner, UniTaskCompletionSource<Poolee> source)
    {
        var spawnable = spawner._spawnable;

        if (spawnable == null || !spawnable.crateRef.IsValid())
        {
            return;
        }

        var transform = spawner.transform;

        NetworkAssetSpawner.Spawn(new NetworkAssetSpawner.SpawnRequestInfo()
        {
            Spawnable = spawnable,
            Position = transform.position,
            Rotation = transform.rotation,
            SpawnCallback = (info) =>
            {
                OnNetworkSpawn(spawner, info, source);
            },
        });
    }

    private static void OnNetworkSpawn(CrateSpawner spawner, NetworkAssetSpawner.SpawnCallbackInfo info, UniTaskCompletionSource<Poolee> source)
    {
        var spawned = info.Spawned;

        var poolee = Poolee.Cache.Get(spawned);

        source.TrySetResult(poolee);

        // Make sure we actually have a network entity
        if (info.Entity == null)
        {
            return;
        }

        // Send spawn message
        var spawnedId = info.Entity.ID;

        CrateSpawnerMessage.SendCrateSpawnerMessage(spawner, spawnedId);
    }

    public static void OnFinishNetworkSpawn(this CrateSpawner spawner, GameObject go)
    {
        // Invoke spawn events
        spawner.onSpawnEvent?.Invoke(spawner, go);

        var poolee = Poolee.Cache.Get(go);

        spawner.OnPooleeSpawn(go);

        poolee.OnDespawnDelegate += (Action<GameObject>)spawner.OnPooleeDespawn;
    }

    private static bool IsSingleplayerOnly(CrateSpawner crateSpawner)
    {
        // Check if this CrateSpawner has a Desyncer
        if (Desyncer.Cache.ContainsSource(crateSpawner.gameObject))
        {
            return true;
        }

        var spawnable = crateSpawner._spawnable;

        if (spawnable == null)
        {
            return false;
        }

        if (!spawnable.crateRef.IsValid() || spawnable.crateRef.Crate == null)
        {
            return false;
        }

        // Check for the Singleplayer Only tag
        if (CrateFilterer.HasTags(spawnable.crateRef.Crate, FusionTags.SingleplayerOnly))
        {
            return true;
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CrateSpawner.SpawnSpawnableAsync))]
    public static bool SpawnSpawnableAsyncPrefix(CrateSpawner __instance, bool isHidden, ref UniTask<Poolee> __result)
    {
        // If this scene is unsynced, the spawner can function as normal.
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        var spawner = __instance;

        if (IsSingleplayerOnly(spawner))
        {
            return true;
        }

        // If we aren't the scene host, don't allow a crate spawn
        if (!NetworkSceneManager.IsLevelHost)
        {
            __result = new UniTask<Poolee>(null);
            return false;
        }

        var source = new UniTaskCompletionSource<Poolee>();
        __result = new UniTask<Poolee>(source.TryCast<IUniTaskSource<Poolee>>(), default);

        // Otherwise, manually sync this spawn over the network
        try
        {
            NetworkedSpawnSpawnable(spawner, source);
        }
        catch (Exception e)
        {
            FusionLogger.LogException($"networking CrateSpawner {spawner.name}", e);
        }

        return false;
    }
}