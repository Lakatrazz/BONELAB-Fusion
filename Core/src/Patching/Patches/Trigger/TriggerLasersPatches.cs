using HarmonyLib;

using System;
using System.Collections.Generic;

using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Data;

using SLZ.Bonelab;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(TriggerLasers), "OnTriggerEnter")]
    public static class PlayerTriggerEnterPatch
    {
        public static bool Prefix(TriggerLasers __instance, Collider other)
        {
            if (other.CompareTag("Player")) {
                if (TriggerUtilities.VerifyLevelTrigger(__instance, other, out bool runMethod))
                    return runMethod;

                TriggerUtilities.Increment(__instance);
                bool canEnter = TriggerUtilities.CanEnter(__instance);

                return canEnter;
            }

            return true;
        }

        public static void Postfix(TriggerLasers __instance, Collider other) {
            if (__instance.rigManager != null && __instance.rigManager != RigData.RigReferences.RigManager)
                __instance.rigManager = RigData.RigReferences.RigManager;
        }
    }

    [HarmonyPatch(typeof(TriggerLasers), "OnTriggerExit")]
    public static class PlayerTriggerExitPatch
    {
        public static bool Prefix(TriggerLasers __instance, Collider other)
        {
            if (other.CompareTag("Player")) {
                if (TriggerUtilities.VerifyLevelTrigger(__instance, other, out bool runMethod))
                    return runMethod;

                TriggerUtilities.Decrement(__instance);
                bool canExit = TriggerUtilities.CanExit(__instance);

                return canExit;
            }

            return true;
        }

        public static void Postfix(TriggerLasers __instance, Collider other) {
            if (__instance.rigManager != null && __instance.rigManager != RigData.RigReferences.RigManager)
                __instance.rigManager = RigData.RigReferences.RigManager;
        }
    }
}

