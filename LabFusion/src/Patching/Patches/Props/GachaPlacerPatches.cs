using HarmonyLib;

using LabFusion.Network;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Props;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(GachaPlacer))]
    public static class GachaPlacerPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GachaPlacer.ShouldSpawn))]
        public static void ShouldSpawn(ref bool __result)
        {
            if (!IgnorePatches && NetworkInfo.HasServer)
            {
                __result = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GachaPlacer.OnSpawn))]
        public static void OnSpawn(GachaPlacer __instance, GameObject go)
        {
            if (NetworkInfo.HasServer)
            {
                IgnorePatches = true;

                bool notUnlocked = __instance.ShouldSpawn();

                if (!notUnlocked)
                {
                    MeshRenderer mesh = go.GetComponent<GachaCapsule>().previewMesh.GetComponent<MeshRenderer>();
                    mesh.material.SetColor("_EmissionColor", Color.gray);
                    mesh.material.SetColor("_HologramEdgeColor", Color.gray);
                }

                IgnorePatches = false;
            }
        }
    }
}
