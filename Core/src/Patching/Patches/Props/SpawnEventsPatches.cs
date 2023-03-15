using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Network;
using SLZ.Bonelab;
using SLZ.Marrow.Pool;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(SpawnEvents))]
    public static class SpawnEventsPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SpawnEvents.Despawn))]
        public static void DespawnPrefix(SpawnEvents __instance, ref bool __state) {
            if (NetworkInfo.HasServer && __instance.TryCast<AmmoPickup>()) {
                AssetPooleePatches.IgnorePatches = true;
                __state = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(SpawnEvents.Despawn))]
        public static void DespawnPostfix(SpawnEvents __instance, bool __state) {
            if (__state) {
                AssetPooleePatches.IgnorePatches = false;
            }
        }
    }
}
