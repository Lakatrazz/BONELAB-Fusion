using HarmonyLib;

using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.Scene;

using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Interaction;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(Poolee), nameof(Poolee.OnDespawnEvent))]
public class PooleeOnDespawnPatch
{
    public static void Postfix(Poolee __instance)
    {
        if (CrossSceneManager.InUnsyncedScene())
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
        FusionLogger.Log($"Unregistered entity at ID {entity.Id} after OnDespawnEvent.");
#endif

        NetworkEntityManager.IdManager.UnregisterEntity(entity);
    }
}

[HarmonyPatch(typeof(Poolee), nameof(Poolee.Despawn))]
public class PooleeDespawnPatch
{
    public static bool IgnorePatch = false;

    private static bool CheckPlayerDespawn(Poolee __instance)
    {
        // Poolee check
        if (PooleeExtender.Cache.TryGet(__instance, out var entity))
        {
            var player = entity.GetExtender<NetworkPlayer>();

            if (player != null)
            {
                FusionLogger.Warn($"Prevented Poolee.Despawn of player at ID {entity.Id}!");
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
                FusionLogger.Warn($"Prevented Poolee.Despawn of player at ID {entity.Id}!");
                return true;
            }
        }

        // Not despawning a player, everything is good
        return false;
    }

    public static bool Prefix(Poolee __instance)
    {
        // Make sure we have a server
        if (CrossSceneManager.InUnsyncedScene())
        {
            return true;
        }

        // Also make sure we're not ignoring this patch
        if (IgnorePatch)
        {
            return true;
        }

        // Don't allow player despawning
        if (CheckPlayerDespawn(__instance))
        {
            return false;
        }

        bool isSceneHost = CrossSceneManager.IsSceneHost();

        // If we are not the scene host, and we don't allow despawns currently, then don't let the entity be despawned
        if (!isSceneHost && !PooleeUtilities.CanDespawn && PooleeExtender.Cache.ContainsSource(__instance))
        {
            return false;
        }

        // If we are the scene host, sync the poolee despawn
        if (isSceneHost)
        {
            CheckForDespawn(__instance);
        }

        return true;
    }

    private static void CheckForDespawn(Poolee __instance)
    {
        if (!PooleeExtender.Cache.TryGet(__instance, out var entity))
        {
            return;
        }

        var prop = entity.GetExtender<NetworkProp>();

        if (prop == null)
        {
            return;
        }

        PooleeUtilities.SendDespawn(entity.Id, false);
        NetworkEntityManager.IdManager.UnregisterEntity(entity);
    }
}