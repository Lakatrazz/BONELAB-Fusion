using System.Collections;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Extensions;
using LabFusion.Entities;

using Il2CppSLZ.Marrow.Pool;

using MelonLoader;


namespace LabFusion.Patching
{
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

            if (PooleeExtender.Cache.TryGet(__instance, out var entity))
            {
                NetworkEntityManager.IdManager.UnregisterEntity(entity);
            }
        }
    }

    [HarmonyPatch(typeof(Poolee), nameof(Poolee.Despawn))]
    public class PooleeDespawnPatch
    {
        public static bool IgnorePatch = false;

        public static bool Prefix(Poolee __instance)
        {
            if (!NetworkInfo.HasServer)
            {
                return true;
            }

            if (PooleeUtilities.IsPlayer(__instance) || IgnorePatch || __instance.IsNOC())
            {
                return true;
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
            if (PooleeExtender.Cache.TryGet(__instance, out var entity))
            {
                PooleeUtilities.SendDespawn(entity.Id);
                NetworkEntityManager.IdManager.UnregisterEntity(entity);
                return true;
            }
            return false;
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
}
