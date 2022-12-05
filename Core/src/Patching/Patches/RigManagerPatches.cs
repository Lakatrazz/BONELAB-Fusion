using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Data;
using SLZ.Rig;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(RigManager))]
    public static class RigManagerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(RigManager.Teleport), typeof(Vector3), typeof(Vector3), typeof(bool))]
        public static void Teleport(RigManager __instance, Vector3 feetDestinationWorld, Vector3 fwdSnap, bool zeroVelocity = true) {
            if (__instance.health._testRagdollOnDeath) {
                __instance.physicsRig.TeleportToPose();
            }
        }
    }
}
