using HarmonyLib;

using Il2CppSLZ.Bonelab;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(DevManipulatorGun))]
    public static class DevManipulatorPatches
    {
        [HarmonyPatch(nameof(DevManipulatorGun.Awake))]
        [HarmonyPostfix]
        public static void Awake(DevManipulatorGun __instance)
        {
            // Add players to layer mask
            var job = __instance.gravityManipulator;
            job.pullableLayerMask |= (1 << LayerMask.NameToLayer("Player"));
        }
    }
}
