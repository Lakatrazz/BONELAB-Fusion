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

namespace LabFusion.Patches
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
        [HarmonyPatch(nameof(ControllerRig.JumpCharge))]
        public static void JumpChargePrefix(ControllerRig __instance, float deltaTime, bool chargeInput, ref bool __state) {
            __state = __instance._chargeInput;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ControllerRig.JumpCharge))]
        public static void JumpChargePostfix(ControllerRig __instance, float deltaTime, bool chargeInput, bool __state) {
            if (NetworkInfo.HasServer && __instance.manager == RigData.RigReferences.RigManager && __state && !__instance._chargeInput) {
                PlayerSender.SendPlayerRepEvent(PlayerRepEventType.JUMP);
            }
        }
    }

    // Here we update controller positions on the reps so they use our desired targets.
    [HarmonyPatch(typeof(OpenControllerRig), "OnFixedUpdate")]
    public class OpenFixedUpdatePatch
    {
        public static void Postfix(OpenControllerRig __instance, float deltaTime, bool __state) {
            try {
                if (PlayerRep.Managers.ContainsKey(__instance.manager))
                {
                    var rep = PlayerRep.Managers[__instance.manager];
                    rep.OnControllerRigUpdate();
                }
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch OpenControllerRig.OnFixedUpdate", e);
#endif
            }
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
                if (__instance.manager != RigData.RigReferences.RigManager)
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
