using System.Collections;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Syncables;
using LabFusion.Utilities;
using LabFusion.Extensions;

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
                return;

            if (NetworkInfo.HasServer && PropSyncable.Cache.TryGet(__instance.gameObject, out var syncable))
            {
                SyncManager.RemoveSyncable(syncable);
            }
        }
    }

    [HarmonyPatch(typeof(Poolee), nameof(Poolee.Despawn))]
    public class PooleeDespawnPatch
    {
        public static bool IgnorePatch = false;

        public static bool Prefix(Poolee __instance)
        {
            if (PooleeUtilities.IsPlayer(__instance) || IgnorePatch || __instance.IsNOC())
                return true;

            try
            {
                if (NetworkInfo.HasServer)
                {
                    if (!NetworkInfo.IsServer && !PooleeUtilities.CanDespawn && PropSyncable.Cache.TryGet(__instance.gameObject, out var syncable))
                    {
                        return false;
                    }
                    else if (NetworkInfo.IsServer)
                    {
                        if (!CheckPropSyncable(__instance) && PooleeUtilities.CheckingForSpawn.Contains(__instance))
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

        private static bool CheckPropSyncable(Poolee __instance)
        {
            if (PropSyncable.Cache.TryGet(__instance.gameObject, out var syncable))
            {
                PooleeUtilities.SendDespawn(syncable.Id);
                SyncManager.RemoveSyncable(syncable);
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

            CheckPropSyncable(__instance);
        }
    }
}
