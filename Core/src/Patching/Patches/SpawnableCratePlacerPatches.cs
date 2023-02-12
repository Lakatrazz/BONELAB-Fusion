using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Utilities;

using MelonLoader;
using SLZ.Marrow.Pool;
using SLZ.Marrow.Warehouse;

using UnityEngine;
using UnityEngine.Events;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(SpawnableCratePlacer))]
    public static class SpawnableCratePlacerPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpawnableCratePlacer.Awake))]
        public static void Awake(SpawnableCratePlacer __instance) {
            var action = (UnityAction)(() => { Internal_OnPlace(__instance); });
            var del = UnityEvent.GetDelegate(action);

            if (__instance.OnPlaceEvent != null)
                __instance.OnPlaceEvent.AddCall(del);
        }

        private static void Internal_OnPlace(SpawnableCratePlacer scp) {
            // Get the last spawned object
            // placedSpawnable is always null
            if (NetworkInfo.IsServer && scp.spawnableCrateReference.Crate != null) {
                var crate = scp.spawnableCrateReference.Crate;

                var barcodeToPool = AssetSpawner._instance._barcodeToPool;

                if (barcodeToPool.ContainsKey(crate.Barcode)) {
                    var pool = barcodeToPool[crate.Barcode];

                    var lastSpawned = pool.spawned[pool.spawned.Count - 1];
                    SpawnSender.SendCratePlacerEvent(scp, lastSpawned.gameObject);
                }
            }
        }
    }
}
