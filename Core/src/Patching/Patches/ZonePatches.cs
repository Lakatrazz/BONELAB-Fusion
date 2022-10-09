using HarmonyLib;

using System;

using LabFusion.Network;
using LabFusion.Extensions;

using SLZ.Zones;

using UnityEngine;

using System.Collections.Generic;

using LabFusion.Utilities;

using SLZ.AI;
using SLZ.Rig;

using LabFusion.Data;

namespace Entanglement.Patching
{
    [HarmonyPatch(typeof(SceneZone), "OnTriggerEnter")]
    public static class ZoneEnterPatch
    {
        public static bool Prefix(SceneZone __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                TriggerUtilities.Increment(__instance);
                bool canEnter = TriggerUtilities.CanEnter(__instance);
#if DEBUG
                FusionLogger.Log($"Entering SceneZone {__instance.name} with number {TriggerUtilities.zoneCount[__instance]} and result {canEnter}");
#endif

                return canEnter;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SceneZone), "OnTriggerExit")]
    public static class ZoneExitPatch
    {
        public static bool Prefix(SceneZone __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                TriggerUtilities.Decrement(__instance);
                bool canExit = TriggerUtilities.CanExit(__instance);

#if DEBUG
                FusionLogger.Log($"Exiting SceneZone {__instance.name} with number {TriggerUtilities.zoneCount[__instance]} and result {canExit}");
#endif

                return canExit;
            }

            return true;
        }
    }
}

