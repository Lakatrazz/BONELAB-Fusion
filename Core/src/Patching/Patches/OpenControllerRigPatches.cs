using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using SLZ.Rig;

using UnityEngine;

using LabFusion.Representation;
using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Network;

using SLZ.Marrow.Input;
using LabFusion.Senders;
using UnityEngine.Rendering;

namespace LabFusion.Patching
{
    // Disables game pausing completely while in a server
    [HarmonyPatch(typeof(XRHMD))]
    public static class XRHMDPatches
    {
        [HarmonyPatch(nameof(XRHMD.IsUserPresent), MethodType.Getter)]
        [HarmonyPostfix]
        public static void IsUserPresent(ref bool __result) {
            if (NetworkInfo.HasServer)
                __result = true;
        }
    }

    [HarmonyPatch(typeof(ControllerRig))]
    public static class ControllerRigPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ControllerRig.OnFixedUpdate))]
        public static void OnFixedUpdate(ControllerRig __instance, float deltaTime)
        {
            try
            {
                if (PlayerRepManager.TryGetPlayerRep(__instance.manager, out var rep))
                {
                    rep.OnControllerRigUpdate();
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch ControllerRig.OnFixedUpdate", e);
#endif
            }
        }
    }

    [HarmonyPatch(typeof(OpenControllerRig))]
    public static class OpenControllerRigPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(OpenControllerRig.OnBeginCameraRendering))]
        public static bool OnBeginCameraRendering(OpenControllerRig __instance, ScriptableRenderContext ctx, Camera cam) {
            if (PlayerRepManager.HasPlayerId(__instance.manager)) {
                return false;
            }
            
            return true;
        }
    }

    // This patch fixes the rig becoming confused due to multiple OnPause state changes.
    [HarmonyPatch(typeof(OpenControllerRig), "OnEarlyUpdate")]
    public class OpenEarlyUpdatePatch
    {
        public static bool Prefix(OpenControllerRig __instance)
        {
            try
            {
                // Check to make sure this isn't the main rig
                if (!__instance.manager.IsLocalPlayer())
                {
                    // Update the time controller to prevent errors
                    if (!__instance.globalTimeControl && RigData.RigReferences.RigManager)
                        __instance.globalTimeControl = RigData.RigReferences.RigManager.openControllerRig.globalTimeControl;

                    // Return false if we are paused
                    if (Time.timeScale <= 0f)
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
