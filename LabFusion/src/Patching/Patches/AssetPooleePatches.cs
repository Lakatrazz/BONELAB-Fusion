﻿using System;
using System.Collections;
using HarmonyLib;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Syncables;
using LabFusion.Utilities;
using MelonLoader;
using SLZ;
using SLZ.Marrow.Pool;
using SLZ.Zones;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(AssetPoolee), nameof(AssetPoolee.OnSpawn))]
    public class PooleeOnSpawnPatch
    {
        private static void CheckRemoveSyncable(AssetPoolee __instance)
        {
            if (PropSyncable.Cache.TryGet(__instance.gameObject, out var syncable))
                SyncManager.RemoveSyncable(syncable);
        }

        public static void Postfix(AssetPoolee __instance, ulong spawnId)
        {
            if (PooleeUtilities.IsPlayer(__instance))
                return;

            try
            {
                if (NetworkInfo.HasServer && __instance.spawnableCrate)
                {
                    var barcode = __instance.spawnableCrate.Barcode;

                    if (!NetworkInfo.IsServer)
                    {
                        // Check if we should prevent this object from spawning
                        if (barcode == CommonBarcodes.FADE_OUT_BARCODE)
                        {
                            __instance.gameObject.SetActive(false);
                        }
                        else if (!PooleeUtilities.ForceEnabled.Contains(__instance) && PooleeUtilities.CanForceDespawn(__instance))
                        {
                            CheckRemoveSyncable(__instance);

                            __instance.gameObject.SetActive(false);
                            MelonCoroutines.Start(CoForceDespawnRoutine(__instance));
                        }
                    }
                    else
                    {
                        if (PooleeUtilities.CanSendSpawn(__instance))
                        {
                            CheckRemoveSyncable(__instance);

                            PooleeUtilities.CheckingForSpawn.Push(__instance);
                            FusionSceneManager.HookOnLevelLoad(() =>
                            {
                                DelayUtilities.Delay(() =>
                                {
                                    OnVerifySpawned(__instance);
                                }, 4);
                            });
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

        private static IEnumerator CoForceDespawnRoutine(AssetPoolee __instance)
        {
            var go = __instance.gameObject;

            for (var i = 0; i < 3; i++)
            {
                yield return null;

                if (!PooleeUtilities.CanForceDespawn(__instance))
                {
                    go.SetActive(true);
                    yield break;
                }

                if (PooleeUtilities.CanSpawnList.Contains(__instance) || PooleeUtilities.ForceEnabled.Contains(__instance))
                    yield break;

                go.SetActive(false);
            }
        }

        private static void OnVerifySpawned(AssetPoolee __instance)
        {
            PooleeUtilities.CheckingForSpawn.Pull(__instance);

            try
            {
                if (PooleeUtilities.CanSendSpawn(__instance) && !PooleeUtilities.ServerSpawnedList.Pull(__instance))
                {
                    var barcode = __instance.spawnableCrate.Barcode;

                    var syncId = SyncManager.AllocateSyncID();
                    PooleeUtilities.OnServerLocalSpawn(syncId, __instance.gameObject, out PropSyncable newSyncable);

                    var zoneTracker = ZoneTracker.Cache.Get(__instance.gameObject);
                    ZoneSpawner spawner = null;

                    if (zoneTracker)
                    {
                        var collection = ZoneSpawner.Cache.m_Cache.Values;

                        // I have to do this garbage, because the ZoneTracker doesn't ever set ZoneTracker.spawner!
                        // Meaning we don't actually know where the fuck this was spawned from!
                        bool breakList = false;

                        foreach (var list in collection)
                        {
                            foreach (var otherSpawner in list)
                            {
                                foreach (var spawnedObj in otherSpawner.spawns)
                                {
                                    if (spawnedObj == __instance.gameObject)
                                    {
                                        spawner = otherSpawner;

                                        breakList = true;
                                        break;
                                    }
                                }

                                if (breakList)
                                    break;
                            }

                            if (breakList)
                                break;
                        }
                    }

                    PooleeUtilities.SendSpawn(0, barcode, syncId, new SerializedTransform(__instance.transform), true, spawner);

                    // Insert catchup hook for future users
                    if (NetworkInfo.IsServer)
                        newSyncable.InsertCatchupDelegate(id =>
                        {
                            SpawnSender.SendCatchupSpawn(0, barcode, syncId, new SerializedTransform(__instance.transform), spawner, Handedness.UNDEFINED, id);
                        });
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute WaitForVerify", e);
#endif
            }
        }
    }

    [HarmonyPatch(typeof(AssetPoolee), nameof(AssetPoolee.OnDespawn))]
    public class PooleeOnDespawnPatch
    {
        public static void Postfix(AssetPoolee __instance)
        {
            if (PooleeUtilities.IsPlayer(__instance) || __instance.IsNOC())
                return;

            if (NetworkInfo.HasServer && PropSyncable.Cache.TryGet(__instance.gameObject, out var syncable))
            {
                SyncManager.RemoveSyncable(syncable);
            }
        }
    }

    [HarmonyPatch(typeof(AssetPoolee), nameof(AssetPoolee.Despawn))]
    public class PooleeDespawnPatch
    {
        public static bool IgnorePatch = false;

        public static bool Prefix(AssetPoolee __instance)
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

                    if (NetworkInfo.IsServer)
                    {
                        if (!CheckPropSyncable(__instance) && PooleeUtilities.CheckingForSpawn.Contains(__instance))
                            MelonCoroutines.Start(CoVerifyDespawnCoroutine(__instance));
                    }
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch AssetPoolee.Despawn", e);
#endif
            }

            return true;
        }

        private static bool CheckPropSyncable(AssetPoolee __instance)
        {
            if (PropSyncable.Cache.TryGet(__instance.gameObject, out var syncable))
            {
                PooleeUtilities.SendDespawn(syncable.Id);
                SyncManager.RemoveSyncable(syncable);
                return true;
            }
            return false;
        }

        private static IEnumerator CoVerifyDespawnCoroutine(AssetPoolee __instance)
        {
            while (!__instance.IsNOC() && PooleeUtilities.CheckingForSpawn.Contains(__instance))
            {
                yield return null;
            }

            CheckPropSyncable(__instance);
        }
    }
}
