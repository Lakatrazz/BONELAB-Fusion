using HarmonyLib;

using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.Scene;
using LabFusion.RPC;

using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(Poolee), nameof(Poolee.OnDespawnEvent))]
public class PooleeOnDespawnPatch
{
    public static void Postfix(Poolee __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (!PooleeExtender.Cache.TryGet(__instance, out var entity))
        {
            return;
        }

        var prop = entity.GetExtender<NetworkProp>();

        if (prop == null)
        {
            return;
        }

#if DEBUG
        FusionLogger.Log($"Unregistered entity at ID {entity.ID} after OnDespawnEvent.");
#endif

        NetworkEntityManager.IDManager.UnregisterEntity(entity);
    }
}

[HarmonyPatch(typeof(Poolee), nameof(Poolee.Despawn))]
public class PooleeDespawnPatch
{
    public static FrameBool IgnorePatch { get; set; } = false;

    private static bool CheckPlayerDespawn(Poolee __instance)
    {
        // Poolee check
        if (PooleeExtender.Cache.TryGet(__instance, out var entity))
        {
            var player = entity.GetExtender<NetworkPlayer>();

            if (player != null)
            {
                FusionLogger.Warn($"Prevented Poolee.Despawn of player at ID {entity.ID}!");
                return true;
            }
        }

        // Marrow Entity check
        var marrowEntity = MarrowEntity.Cache.Get(__instance.gameObject);

        if (marrowEntity != null && IMarrowEntityExtender.Cache.TryGet(marrowEntity, out entity))
        {
            var player = entity.GetExtender<NetworkPlayer>();

            if (player != null)
            {
                FusionLogger.Warn($"Prevented Poolee.Despawn of player at ID {entity.ID}!");
                return true;
            }
        }

        // Not despawning a player, everything is good
        return false;
    }

    public static bool Prefix(Poolee __instance)
    {
        // Make sure we have a server
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        // Don't allow player despawning
        if (CheckPlayerDespawn(__instance))
        {
            return false;
        }

        // Also make sure we're not ignoring this patch
        if (IgnorePatch)
        {
            return true;
        }

        if (!PooleeExtender.Cache.TryGet(__instance, out var networkEntity))
        {
            return true;
        }

        if (networkEntity.GetExtender<NetworkProp>() == null)
        {
            return true;
        }

        bool hasOwnership = NetworkSceneManager.IsLevelHost || networkEntity.IsOwner;

        if (!hasOwnership)
        {
            return false;
        }

        NetworkAssetSpawner.Despawn(new NetworkAssetSpawner.DespawnRequestInfo()
        {
            EntityID = networkEntity.ID,
            DespawnEffect = false,
        });

        return false;
    }
}