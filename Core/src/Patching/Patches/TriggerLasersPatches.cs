using HarmonyLib;

using System;

using LabFusion.Network;
using LabFusion.Extensions;

using SLZ.Zones;

using UnityEngine;

using System.Collections.Generic;

using LabFusion.Utilities;
using LabFusion.Data;

using SLZ.AI;
using SLZ.Rig;

namespace Entanglement.Patching
{
    [HarmonyPatch(typeof(TriggerLasers), "OnTriggerEnter")]
    public static class PlayerTriggerEnterPatch
    {
        public static bool Prefix(TriggerLasers __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Regular trigger events
                if (!__instance.onlyTriggerOnPlayer)
                {
                    TriggerUtilities.Increment(__instance);
                    bool canEnter = TriggerUtilities.CanEnter(__instance);
#if DEBUG
                    FusionLogger.Log($"Entering TriggerLasers {__instance.name} with number {TriggerUtilities.triggerCount[__instance]} and result {canEnter}");
#endif

                    return canEnter;
                }
                // Only let the main player enter
                else
                {
                    var trigger = TriggerRefProxy.Cache.Get(other.gameObject);
                    RigManager rig;

                    if (trigger && trigger.root && (rig = RigManager.Cache.Get(trigger.root)))
                    {
                        return rig == RigData.RigManager;
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(TriggerLasers), "OnTriggerExit")]
    public static class PlayerTriggerExitPatch
    {
        public static bool Prefix(TriggerLasers __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Regular trigger events
                if (!__instance.onlyTriggerOnPlayer)
                {
                    TriggerUtilities.Decrement(__instance);
                    bool canExit = TriggerUtilities.CanExit(__instance);

#if DEBUG
                    FusionLogger.Log($"Exiting TriggerLasers {__instance.name} with number {TriggerUtilities.triggerCount[__instance]} and result {canExit}");
#endif

                    return canExit;
                }
                // Only exit if its the main player
                else
                {
                    var trigger = TriggerRefProxy.Cache.Get(other.gameObject);
                    RigManager rig;

                    if (trigger && trigger.root && (rig = RigManager.Cache.Get(trigger.root)))
                    {
                        return rig == RigData.RigManager;
                    }
                }
            }

            return true;
        }
    }
}

