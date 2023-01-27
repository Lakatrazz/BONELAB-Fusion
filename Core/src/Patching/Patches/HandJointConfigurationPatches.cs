using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;

using SLZ.Interaction;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(HandJointConfiguration))]
    public static class HandJointConfigurationPatches
    {
        private static void Internal_ApplyJointSettings(ConfigurableJoint joint) {
            joint.breakForce = float.PositiveInfinity;
            joint.breakTorque = float.PositiveInfinity;

            if (NetworkInfo.HasServer) {
                var hand = Hand.Cache.Get(joint.gameObject);

                if (hand && PlayerRepManager.HasPlayerId(hand.manager)) {
                    joint.breakForce = float.PositiveInfinity;
                    joint.breakTorque = float.PositiveInfinity;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(HandJointConfiguration.LockConfiguration))]
        public static void LockConfiguration(ConfigurableJoint joint)
        {
            Internal_ApplyJointSettings(joint);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(HandJointConfiguration.ApplyConfiguration), typeof(ConfigurableJoint))]
        public static void ApplyConfiguration(ConfigurableJoint joint)
        {
            Internal_ApplyJointSettings(joint);
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(HandJointConfiguration.ApplyConfiguration), typeof(Quaternion), typeof(ConfigurableJoint))]
        public static void ApplyConfiguration(Quaternion localRotation, ConfigurableJoint joint)
        {
            Internal_ApplyJointSettings(joint);
        }
    }
}
