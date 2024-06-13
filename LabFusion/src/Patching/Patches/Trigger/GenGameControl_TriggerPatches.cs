using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;
using Il2CppSLZ.Bonelab;

using UnityEngine;
using Il2CppSLZ.Marrow.AI;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(GenGameControl_Trigger))]
    public static class GenGameControl_TriggerPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(GenGameControl_Trigger.OnTriggerEnter))]
        public static bool OnTriggerEnter(GenGameControl_Trigger __instance, Collider other)
        {
            if (!NetworkInfo.HasServer)
            {
                return true;
            }

            var proxy = other.GetComponent<TriggerRefProxy>();

            if (proxy == null)
            {
                return true;
            }

            if (proxy.triggerType == TriggerRefProxy.TriggerType.Player)
            {
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
        public static bool OnTriggerExit(GenGameControl_Trigger __instance, Collider other)
        {
            if (!NetworkInfo.HasServer)
            {
                return true;
            }

            var proxy = other.GetComponent<TriggerRefProxy>();

            if (proxy == null)
            {
                return true;
            }

            if (proxy.triggerType == TriggerRefProxy.TriggerType.Player)
            {
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
