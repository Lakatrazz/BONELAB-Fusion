using HarmonyLib;

using Il2CppSLZ.Rig;
using Il2CppSLZ.Marrow.Input;

using UnityEngine;

using LabFusion.Network;
using LabFusion.Entities;
using LabFusion.Utilities;

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

    // Removes pause hooks for external players, so they dont break your camera
    [HarmonyPatch(typeof(OpenControllerRig), nameof(OpenControllerRig.OnEarlyUpdate))]
    public static class OpenEarlyUpdatePatch
    {
        public static void Prefix(OpenControllerRig __instance, ref Il2CppSystem.Action<bool> __state)
        {
            __state = OpenControllerRig.OnPauseStateChange;

            // If this isn't our player, the pause callback should not be called
            if (!__instance.manager.IsSelf())
            {
                OpenControllerRig.OnPauseStateChange = null;
            }
        }

        public static void Postfix(ref Il2CppSystem.Action<bool> __state)
        {
            OpenControllerRig.OnPauseStateChange = __state;
        }
    }

    // Overrides hand and headset positions for external players
    [HarmonyPatch(typeof(OpenControllerRig), nameof(OpenControllerRig.OnRealHeptaEarlyUpdate))]
    public static class OpenRealHeptaEarlyUpdatePatch
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
