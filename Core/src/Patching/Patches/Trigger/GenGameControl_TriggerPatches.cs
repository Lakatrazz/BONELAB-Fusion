using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.AI;
using SLZ.Bonelab;
using SLZ.Zones;

using UnityEngine;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(GenGameControl_Trigger))]
    public static class GenGameControl_TriggerPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GenGameControl_Trigger.OnTriggerEnter))]
        public static bool OnTriggerEnter(GenGameControl_Trigger __instance, Collider other) {
            if (NetworkInfo.HasServer && other.CompareTag("Player")) {
                if (TriggerUtilities.VerifyLevelTrigger(__instance, other, out bool runMethod))
                    return runMethod;

                TriggerUtilities.Increment(__instance);
                bool canEnter = TriggerUtilities.CanEnter(__instance);

                return canEnter;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GenGameControl_Trigger.OnTriggerExit))]
        public static bool OnTriggerExit(GenGameControl_Trigger __instance, Collider other) {
            if (NetworkInfo.HasServer && other.CompareTag("Player")) {
                if (TriggerUtilities.VerifyLevelTrigger(__instance, other, out bool runMethod))
                    return runMethod;

                TriggerUtilities.Decrement(__instance);
                bool canExit = TriggerUtilities.CanExit(__instance);

                return canExit;
            }

            return true;
        }
    }
}
