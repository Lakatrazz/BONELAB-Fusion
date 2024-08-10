using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;

using UnityEngine.Events;

using Il2CppSLZ.Marrow.Zones;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(MobileEncounter))]
    public static class ZoneEncounterPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MobileEncounter.Awake))]
        public static void Awake(MobileEncounter __instance)
        {
            var onComplete = () => { OnComplete(__instance); };
            __instance.OnComplete.add_DynamicCalls(onComplete);
        }

        // CompleteEncounter is inlined into the coroutine, so we manually use the UnityEvent
        public static void OnComplete(MobileEncounter __instance)
        {
            if (IgnorePatches)
                return;

            // Only sync the complete event if we are the server
            if (NetworkInfo.IsServer)
            {
                ZoneSender.SendZoneEncounterEvent(__instance, ZoneEncounterEventType.COMPLETE_ENCOUNTER);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MobileEncounter.StartEncounter))]
        [HarmonyPatch(new Type[] {})]
        public static bool StartEncounter(MobileEncounter __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                    ZoneSender.SendZoneEncounterEvent(__instance, ZoneEncounterEventType.START_ENCOUNTER);
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(MobileEncounter.PauseEncounter))]
        public static bool PauseEncounter(MobileEncounter __instance)
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
