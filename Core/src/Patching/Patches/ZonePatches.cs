using HarmonyLib;

using SLZ.Zones;

using UnityEngine;

using LabFusion.Utilities;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(SceneZone), nameof(SceneZone.OnTriggerEnter))]
    public static class ZoneEnterPatch
    {
        public static bool Prefix(SceneZone __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                return TriggerUtilities.IsMainRig(other);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SceneZone), nameof(SceneZone.OnTriggerExit))]
    public static class ZoneExitPatch
    {
        public static bool Prefix(SceneZone __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                return TriggerUtilities.IsMainRig(other);
            }

            return true;
        }
    }
}

