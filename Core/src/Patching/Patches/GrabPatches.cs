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

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Hand), "OnPhysRigUpdate")]
    public static class OnPhysRigUpdatePatch {
        public static void Postfix(Hand __instance) {
            if (!__instance.m_CurrentAttachedGO)
                return;

            if (PlayerRep.Managers.ContainsKey(__instance.manager) && __instance.tempJoint) {
                __instance.tempJoint.xMotion = __instance.tempJoint.yMotion = __instance.tempJoint.zMotion 
                    = __instance.tempJoint.angularXMotion = __instance.tempJoint.angularYMotion = __instance.tempJoint.angularZMotion = ConfigurableJointMotion.Locked;
            }            
        }

    }

    [HarmonyPatch(typeof(Hand), "AttachObject")]
    public static class AttachObjectPatch
    {
        public static void Prefix(Hand __instance, GameObject objectToAttach) {
            // Make sure this is the main rig
            if (__instance.manager != RigData.RigReferences.RigManager)
                return;

            PlayerRepUtilities.SendObjectAttach(__instance.handedness, Grip.Cache.Get(objectToAttach));
        }
    }

    [HarmonyPatch(typeof(Hand), "DetachObject")]
    public static class DetachObjectPatch
    {
        public static void Prefix(Hand __instance) {
            // Make sure this is the main rig
            if (__instance.manager != RigData.RigReferences.RigManager)
                return;

            // Make sure we actually have something to detach
            if (!__instance.HasAttachedObject())
                return;

            PlayerRepUtilities.SendObjectDetach(__instance.handedness);
        }
    }

}
