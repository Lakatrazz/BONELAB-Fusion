using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Pool;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(Poolee), nameof(Poolee.OnDespawnEvent))]
public class PooleeOnDespawnPatch
{
    public static void Postfix(Poolee __instance)
    {
        if (!NetworkInfo.HasServer)
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

    public static bool Prefix(Poolee __instance)
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        // Also make sure we're not ignoring this patch
        if (IgnorePatch)
        {
            return true;
        }

        // Prevent despawning of other players
        if (PooleeExtender.Cache.TryGet(__instance, out var entity))
        {
            var player = entity.GetExtender<NetworkPlayer>();

            if (player != null && !entity.IsOwner)
            {
                FusionLogger.Warn($"Prevented despawn of player at ID {entity.Id}!");
                return false;
            }
        }

        // If we are not a server, and we don't allow despawns currently, then don't let the entity be despawned
        if (!NetworkInfo.IsServer && !PooleeUtilities.CanDespawn && PooleeExtender.Cache.ContainsSource(__instance))
        {
            return false;
        }

        // If we are the server, sync the poolee despawn
        if (NetworkInfo.IsServer)
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

        PooleeUtilities.SendDespawn(entity.Id);
        NetworkEntityManager.IdManager.UnregisterEntity(entity);
    }
}