using HarmonyLib;

using UnityEngine;

using LabFusion.Utilities;

using Il2CppSLZ.Marrow.Zones;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Zone), nameof(Zone.OnTriggerEnter))]
    public static class ZoneEnterPatch
    {
        public static bool Prefix(Zone __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                return TriggerUtilities.IsMainRig(other);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Zone), nameof(Zone.OnTriggerExit))]
    public static class ZoneExitPatch
    {
        public static bool Prefix(Zone __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                return TriggerUtilities.IsMainRig(other);
            }

            return true;
        }
    }
}

