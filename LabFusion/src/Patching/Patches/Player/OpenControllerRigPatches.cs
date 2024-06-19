using HarmonyLib;

using Il2CppSLZ.Rig;
using Il2CppSLZ.Marrow.Input;

using UnityEngine;

using LabFusion.Network;
using LabFusion.Entities;

using UnityEngine.Rendering;

namespace LabFusion.Patching
{
    // Disables game pausing completely while in a server
    [HarmonyPatch(typeof(XRHMD))]
    public static class XRHMDPatches
    {
        [HarmonyPatch(nameof(XRHMD.IsUserPresent), MethodType.Getter)]
        [HarmonyPostfix]
        public static void IsUserPresent(ref bool __result)
        {
            if (NetworkInfo.HasServer)
                __result = true;
        }
    }

    [HarmonyPatch(typeof(OpenControllerRig))]
    public static class OpenControllerRigPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(OpenControllerRig.OnBeginCameraRendering))]
        public static bool OnBeginCameraRendering(OpenControllerRig __instance, ScriptableRenderContext ctx, Camera cam)
        {
            if (NetworkPlayerManager.HasExternalPlayer(__instance.manager))
            {
                return false;
            }

            return true;
        }
    }

    // Syncs hand and headset positions for player reps
    [HarmonyPatch(typeof(OpenControllerRig), nameof(OpenControllerRig.OnRealHeptaEarlyUpdate))]
    public class OpenEarlyUpdatePatch
    {
        public static void Prefix(OpenControllerRig __instance, float deltaTime)
        {
            if (NetworkPlayerManager.TryGetPlayer(__instance.manager, out var player) && !player.NetworkEntity.IsOwner)
            {
                player.OnOverrideControllerRig();
            }
        }
    }
}
