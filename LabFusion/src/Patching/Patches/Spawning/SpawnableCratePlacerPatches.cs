using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;

using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(CrateSpawner))]
    public static class SpawnableCratePlacerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(CrateSpawner.Awake))]
        public static void Awake(CrateSpawner __instance)
        {
            var action = (Il2CppSystem.Action<CrateSpawner, GameObject>)((c, g) => { Internal_OnPlace(__instance); });

            if (__instance.onSpawnEvent != null)
                __instance.onSpawnEvent.add_DynamicCalls(action);
        }

        private static void Internal_OnPlace(CrateSpawner scp)
        {
            // Get the last spawned object
            // placedSpawnable is always null
            if (NetworkInfo.IsServer && scp.spawnableCrateReference.Crate != null)
            {
                var crate = scp.spawnableCrateReference.Crate;

                var barcodeToPool = AssetSpawner._instance._barcodeToPool;

                if (barcodeToPool.ContainsKey(crate.Barcode))
                {
                    var pool = barcodeToPool[crate.Barcode];

                    var lastSpawned = pool._spawned[pool._spawned.Count - 1];
                    SpawnSender.SendCratePlacerEvent(scp, lastSpawned.gameObject);
                }
            }
        }
    }
}
