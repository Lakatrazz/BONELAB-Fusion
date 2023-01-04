using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;

using SLZ.Zones;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(ZoneEncounter))]
    public static class ZoneEncounterPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ZoneEncounter.StartEncounter))]
        public static bool StartEncounter(ZoneEncounter __instance) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (NetworkInfo.IsServer)
                    ZoneSender.SendZoneEncounterEvent(ZoneEncounterEventType.START_ENCOUNTER, __instance);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ZoneEncounter.PauseEncounter))]
        public static bool PauseEncounter(ZoneEncounter __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    ZoneSender.SendZoneEncounterEvent(ZoneEncounterEventType.PAUSE_ENCOUNTER, __instance);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ZoneEncounter.CompleteEncounter))]
        public static bool CompleteEncounter(ZoneEncounter __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    ZoneSender.SendZoneEncounterEvent(ZoneEncounterEventType.COMPLETE_ENCOUNTER, __instance);
                else
                    return false;
            }

            return true;
        }
    }
}
