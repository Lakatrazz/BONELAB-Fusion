using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;

using SLZ.Marrow.Warehouse;
using SLZ.Props;

using UnityEngine;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(GachaPlacer))]
    public static class GachaPlacerPatches {
        public static bool IgnorePatches = false;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GachaPlacer.ShouldPlace))]
        public static void ShouldPlace(ref bool __result) {
            if (!IgnorePatches && NetworkInfo.HasServer) {
                __result = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GachaPlacer.SetGachaContents))]
        public static void SetGachaContents(GachaPlacer __instance, SpawnableCratePlacer scp, GameObject go) {
            if (NetworkInfo.HasServer) {
                IgnorePatches = true;

                bool notUnlocked = __instance.ShouldPlace();

                if (!notUnlocked) {
                    MeshRenderer mesh = GachaCapsule.Cache.Get(go).previewMesh.GetComponent<MeshRenderer>();
                    mesh.material.SetColor("_EmissionColor", Color.gray);
                    mesh.material.SetColor("_HologramEdgeColor", Color.gray);
                }

                IgnorePatches = false;
            }
        }
    }
}
