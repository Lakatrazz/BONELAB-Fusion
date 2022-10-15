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

namespace LabFusion.Patches
{
    // Here we update controller positions on the reps so they use our desired targets.
    [HarmonyPatch(typeof(OpenControllerRig), "OnFixedUpdate")]
    public class OpenFixedUpdatePatch
    {
        public static void Postfix(OpenControllerRig __instance, float deltaTime) {
            if (PlayerRep.Managers.ContainsKey(__instance.manager)) {
                var rep = PlayerRep.Managers[__instance.manager];
                rep.OnUpdateTransforms();
                rep.OnUpdateVelocity();
            }
        }
    }

    // This patch fixes the rig becoming confused due to multiple OnPause state changes.
    [HarmonyPatch(typeof(OpenControllerRig), "OnEarlyUpdate")]
    public class OpenEarlyUpdatePatch
    {
        public static bool Prefix(OpenControllerRig __instance)
        {
            // Check to make sure this isn't the main rig
            if (__instance.manager != RigData.RigReferences.RigManager) {
                // Update the time controller to prevent errors
                if (!__instance.globalTimeControl && RigData.RigReferences.RigManager)
                    __instance.globalTimeControl = RigData.RigReferences.RigManager.openControllerRig.globalTimeControl;

                // Return false if we are paused
                if (Time.timeScale <= 0f)
                    return false;
            }

            return true;
        }
    }
}
