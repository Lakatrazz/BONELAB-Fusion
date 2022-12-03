using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;
using LabFusion.Data;

using SLZ.Marrow.Pool;

using UnityEngine;
using static MelonLoader.MelonLogger;
using MelonLoader;
using SLZ.Marrow.Warehouse;
using SLZ.Zones;
using LabFusion.Extensions;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(AssetPoolee), nameof(AssetPoolee.OnSpawn))]
    public class PooleeOnSpawnPatch {
        public static void Postfix(AssetPoolee __instance, ulong spawnId) {
            if (PooleeUtilities.IsPlayer(__instance))
                return;

            try {
                if (NetworkInfo.HasServer && __instance.spawnableCrate)
                {
                    var barcode = __instance.spawnableCrate.Barcode;

                    if (!NetworkInfo.IsServer)
                    {
                        // Check if we should prevent this object from spawning
                        if (barcode == SpawnableWarehouseUtilities.FADE_OUT_BARCODE) {
                            __instance.gameObject.SetActive(false);
                        }
                        else if (!PooleeUtilities.IsForceEnabled(__instance) && PooleeUtilities.CanForceDespawn(__instance)) {
                            __instance.gameObject.SetActive(false);
                            MelonCoroutines.Start(CoForceDespawnRoutine(__instance));
                        }
                    }
                    else
                    {
                        if (PooleeUtilities.CanSendSpawn(__instance)) {
                            if (PropSyncable.Cache.TryGetValue(__instance.gameObject, out var syncable))
                                SyncManager.RemoveSyncable(syncable);

                            MelonCoroutines.Start(CoVerifySpawnedRoutine(__instance));
                        }
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch AssetPoolee.OnSpawn", e);
#endif
            }
        }

        private static IEnumerator CoForceDespawnRoutine(AssetPoolee __instance) {
            var go = __instance.gameObject;

            for (var i = 0; i < 3; i++) {
                yield return null;

                if (!PooleeUtilities.CanForceDespawn(__instance)) {
                    go.SetActive(true);
                    yield break;
                }

                if (PooleeUtilities.CanSpawn(__instance) || PooleeUtilities.IsForceEnabled(__instance))
                    yield break;

                go.SetActive(false);
            }
        }

        private static IEnumerator CoVerifySpawnedRoutine(AssetPoolee __instance) {
            while (LevelWarehouseUtilities.IsLoading())
                yield return null;

            for (var i = 0; i < 4; i++)
                yield return null;

            try
            {
                if (PooleeUtilities.CanSendSpawn(__instance) && !PooleeUtilities.DequeueServerSpawned(__instance))
                {
                    var barcode = __instance.spawnableCrate.Barcode;

                    var syncId = SyncManager.AllocateSyncID();
                    PooleeUtilities.OnServerLocalSpawn(syncId, __instance.gameObject);

                    var zoneTracker = ZoneTracker.Cache.Get(__instance.gameObject);
                    ZoneSpawner spawner = null;

                    if (zoneTracker && zoneTracker.spawner) {
                        spawner = zoneTracker.spawner;
                    }

                    PooleeUtilities.SendSpawn(0, barcode, syncId, new SerializedTransform(__instance.transform), true, spawner);
                }
            }
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("to execute WaitForVerify", e);
#endif
            }
        }
    }

    [HarmonyPatch(typeof(AssetPoolee), nameof(AssetPoolee.Despawn))]
    public class PooleeDespawnPatch {
        public static bool Prefix(AssetPoolee __instance) {
            if (PooleeUtilities.IsPlayer(__instance))
                return true;

            try {
                if (NetworkInfo.HasServer && !__instance.IsNOC() && !__instance.gameObject.IsNOC() && PropSyncable.Cache.TryGetValue(__instance.gameObject, out var syncable)) {
                    if (!NetworkInfo.IsServer && !PooleeUtilities.CanDespawn) {
                        return false;
                    }
                    else if (NetworkInfo.IsServer) {
                        PooleeUtilities.SendDespawn(syncable.Id);
                    }
                }
            } 
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("to execute patch AssetPoolee.Despawn", e);
#endif
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(AssetPoolee), nameof(AssetPoolee.OnDespawn))]
    public class PooleeOnDespawnPatch {
        public static void Postfix(AssetPoolee __instance) {
            if (PooleeUtilities.IsPlayer(__instance))
                return;

            try {
                if (NetworkInfo.HasServer && !__instance.IsNOC() && !__instance.gameObject.IsNOC() && PropSyncable.Cache.TryGetValue(__instance.gameObject, out var syncable)) {
                    SyncManager.RemoveSyncable(syncable);
                }
            } 
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("to execute patch AssetPoolee.OnDespawn", e);
#endif
            }
        }
    }
}
