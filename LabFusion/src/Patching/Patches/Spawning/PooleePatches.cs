using System.Collections;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Extensions;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Pool;

using MelonLoader;


namespace LabFusion.Patching;

[HarmonyPatch(typeof(Poolee), nameof(Poolee.OnDespawnEvent))]
public class PooleeOnDespawnPatch
{
    public static void Postfix(Poolee __instance)
    {
        if (PooleeUtilities.IsPlayer(__instance) || __instance.IsNOC())
        {
            return;
        }

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

        // Prevent despawning of players
        if (PooleeUtilities.IsPlayer(__instance))
        {
            FusionLogger.Warn($"Prevented despawn of RigManager {__instance.name}!");
            return false;
        }

        try
        {
            if (!NetworkInfo.IsServer && !PooleeUtilities.CanDespawn && PooleeExtender.Cache.ContainsSource(__instance))
            {
                return false;
            }
            else if (NetworkInfo.IsServer)
            {
                if (!CheckNetworkEntity(__instance) && PooleeUtilities.CheckingForSpawn.Contains(__instance))
                {
                    MelonCoroutines.Start(CoVerifyDespawnCoroutine(__instance));
                }
            }
        }
        catch (Exception e)
        {
#if DEBUG
            FusionLogger.LogException("to execute patch Poolee.Despawn", e);
#endif
        }

        return true;
    }

    private static bool CheckNetworkEntity(Poolee __instance)
    {
        if (!PooleeExtender.Cache.TryGet(__instance, out var entity))
        {
            return false;
        }

        var prop = entity.GetExtender<NetworkProp>();

        if (prop == null)
        {
            return false;
        }

        PooleeUtilities.SendDespawn(entity.Id);
        NetworkEntityManager.IdManager.UnregisterEntity(entity);
        return true;
    }

    private static IEnumerator CoVerifyDespawnCoroutine(Poolee __instance)
    {
        while (!__instance.IsNOC() && PooleeUtilities.CheckingForSpawn.Contains(__instance))
        {
            yield return null;
        }

        CheckNetworkEntity(__instance);
    }
}