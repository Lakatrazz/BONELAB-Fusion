using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Syncables;

using SLZ.Marrow.Pool;
using SLZ.Rig;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ConfigurableJoint), MethodType.Setter)]
    public static class ConfigurableJointDrivePatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ConfigurableJoint.xDrive))]
        public static void XDrive(ConfigurableJoint __instance, ref JointDrive value) {
            __instance.CheckSyncableDrive(JointExtensions.JointDriveAxis.XDrive, ref value);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ConfigurableJoint.yDrive))]
        public static void YDrive(ConfigurableJoint __instance, ref JointDrive value) {
            __instance.CheckSyncableDrive(JointExtensions.JointDriveAxis.YDrive, ref value);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ConfigurableJoint.zDrive))]
        public static void ZDrive(ConfigurableJoint __instance, ref JointDrive value) {
            __instance.CheckSyncableDrive(JointExtensions.JointDriveAxis.ZDrive, ref value);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ConfigurableJoint.angularXDrive))]
        public static void AngularXDrive(ConfigurableJoint __instance, ref JointDrive value) {
            __instance.CheckSyncableDrive(JointExtensions.JointDriveAxis.AngularXDrive, ref value);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ConfigurableJoint.angularYZDrive))]
        public static void AngularYZDrive(ConfigurableJoint __instance, ref JointDrive value) {
            __instance.CheckSyncableDrive(JointExtensions.JointDriveAxis.AngularYZDrive, ref value);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ConfigurableJoint.slerpDrive))]
        public static void SlerpDrive(ConfigurableJoint __instance, ref JointDrive value) {
            __instance.CheckSyncableDrive(JointExtensions.JointDriveAxis.SlerpDrive, ref value);
        }
    }
}
