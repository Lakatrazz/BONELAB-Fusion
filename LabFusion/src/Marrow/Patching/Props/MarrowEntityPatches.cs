using HarmonyLib;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Zones;
using Il2CppSLZ.VRMK;

using LabFusion.Data;
using LabFusion.Entities;
using LabFusion.MonoBehaviours;
using LabFusion.Scene;
using LabFusion.Utilities;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(MarrowEntity))]
public static class MarrowEntityPatches
{
    public static readonly ComponentHashTable<MarrowEntity> HashTable = new();

    private static bool IsPooled(MarrowEntity entity)
    {
        var poolee = entity._poolee;

        if (poolee == null)
        {
            return false;
        }

        if (!poolee.IsInPool)
        {
            return false;
        }

        return true;
    }

    private static bool IsRuntimeCreated(MarrowEntity entity)
    {
        if (entity.GetComponentInParent<Avatar>())
        {
            return true;
        }

        if (entity.GetComponentInParent<AntiHasher>())
        {
            return true;
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MarrowEntity.Awake))]
    public static void Awake(MarrowEntity __instance)
    {
        // Don't hash pooled entities, we automatically network spawned objects
        // Hashing pooled entities can cause more conflicts
        if (IsPooled(__instance))
        {
            return;
        }

        // Same with any other runtime created entities (modded avatars, for example)
        if (IsRuntimeCreated(__instance))
        {
            return;
        }

        var hash = GameObjectHasher.GetHierarchyHash(__instance.gameObject);

        var index = HashTable.AddComponent(hash, __instance);

#if DEBUG
        if (index > 0)
        {
            FusionLogger.Log($"Entity {__instance.name} had a conflicting hash {hash} and has been added at index {index}.");
        }
#endif
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MarrowEntity.OnDestroy))]
    public static void OnDestroy(MarrowEntity __instance)
    {
        HashTable.RemoveComponent(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(MarrowEntity.OnCullResolve))]
    public static void OnCullResolve(MarrowEntity __instance, InactiveStatus status, bool isInactive)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var entity = IMarrowEntityExtender.Cache.Get(__instance);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<IMarrowEntityExtender>();

        extender.OnEntityCull(isInactive);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MarrowEntity.Despawn))]
    public static bool Despawn(MarrowEntity __instance)
    {
        // Make sure the level is networked
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        // Prevent despawning of players
        if (IMarrowEntityExtender.Cache.TryGet(__instance, out var entity))
        {
            var player = entity.GetExtender<NetworkPlayer>();

            if (player != null)
            {
                FusionLogger.Warn($"Prevented MarrowEntity.Despawn of player at ID {entity.ID}!");
                return false;
            }
        }

        return true;
    }
}
