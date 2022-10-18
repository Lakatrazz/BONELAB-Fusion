using SLZ.Interaction;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Hand), "AttachObject")]
    public static class AttachObjectPatch
    {
        public static void Prefix(Hand __instance, GameObject objectToAttach) {
            try {
                // Make sure this is the main rig
                if (__instance.manager != RigData.RigReferences.RigManager)
                    return;

                GrabHelper.SendObjectAttach(__instance.handedness, Grip.Cache.Get(objectToAttach));
            }
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("to execute patch Hand.AttachObject", e);
#endif
            }
        }
    }

    [HarmonyPatch(typeof(Hand), "DetachObject")]
    public static class DetachObjectPatch
    {
        public static void Prefix(Hand __instance) {
            try {
                // Make sure this is the main rig
                if (__instance.manager != RigData.RigReferences.RigManager)
                    return;

                // Make sure we actually have something to detach
                if (!__instance.HasAttachedObject())
                    return;

                GrabHelper.SendObjectDetach(__instance.handedness);
            }
            catch (Exception e) {
#if DEBUG
                FusionLogger.LogException("to execute patch Hand.DetachObject", e);
#endif
            }
        }
    }

}
