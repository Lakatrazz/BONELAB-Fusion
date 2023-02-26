using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;

using SLZ.Zones;

using UnityEngine.Events;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(ZoneEncounter))]
    public static class ZoneEncounterPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ZoneEncounter.Awake))]
        public static void Awake(ZoneEncounter __instance) {
            __instance.OnComplete.AddListener((UnityAction)(() => { OnComplete(__instance); }));
        }

        // CompleteEncounter is inlined into the coroutine, so we manually use the UnityEvent
        public static void OnComplete(ZoneEncounter __instance) {
            if (IgnorePatches)
                return;

            // Only sync the complete event if we are the server
            if (NetworkInfo.IsServer) {
                ZoneSender.SendZoneEncounterEvent(__instance, ZoneEncounterEventType.COMPLETE_ENCOUNTER);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ZoneEncounter.StartEncounter))]
        public static bool StartEncounter(ZoneEncounter __instance) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (NetworkInfo.IsServer)
                    ZoneSender.SendZoneEncounterEvent(__instance, ZoneEncounterEventType.START_ENCOUNTER);
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
                    ZoneSender.SendZoneEncounterEvent(__instance, ZoneEncounterEventType.PAUSE_ENCOUNTER);
                else
                    return false;
            }

            return true;
        }
    }
}
