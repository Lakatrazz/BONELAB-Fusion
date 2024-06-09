using HarmonyLib;

using Il2CppSLZ.Rig;
using Il2CppSLZ.Marrow.Input;

using UnityEngine;

using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Network;

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

//     [HarmonyPatch(typeof(ControllerRig))]
//     public static class ControllerRigPatches
//     {
//         [HarmonyPrefix]
//         [HarmonyPatch(nameof(ControllerRig.OnFixedUpdate))]
//         public static void OnFixedUpdate(ControllerRig __instance, float deltaTime)
//         {
//             try
//             {
//                 if (PlayerRepManager.TryGetPlayerRep(__instance.manager, out var rep))
//                 {
//                     rep.OnControllerRigUpdate();
//                 }
//             }
//             catch (Exception e)
//             {
// #if DEBUG
//                 FusionLogger.LogException("to execute patch ControllerRig.OnFixedUpdate", e);
// #endif
//             }
//         }
//     }

    [HarmonyPatch(typeof(OpenControllerRig))]
    public static class OpenControllerRigPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(OpenControllerRig.OnBeginCameraRendering))]
        public static bool OnBeginCameraRendering(OpenControllerRig __instance, ScriptableRenderContext ctx, Camera cam)
        {
            if (PlayerRepManager.HasPlayerId(__instance.manager))
            {
                return false;
            }

            return true;
        }
    }

    // This patch fixes the rig becoming confused due to multiple OnPause state changes.
    [HarmonyPatch(typeof(OpenControllerRig), nameof(OpenControllerRig.OnEarlyUpdate))]
    public class OpenEarlyUpdatePatch
    {
        public static bool Prefix(OpenControllerRig __instance)
        {
            if (PlayerRepManager.TryGetPlayerRep(__instance.manager, out var rep))
            {
                rep.OnControllerRigUpdate();
            }

            try
            {
                // Check to make sure this isn't the main rig
                if (!__instance.manager.IsSelf())
                {
                    // Return false if we are paused
                    if (TimeUtilities.TimeScale <= 0f)
                        return false;
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch OpenControllerRig.OnEarlyUpdate", e);
#endif
            }

            return true;
        }
    }
}
