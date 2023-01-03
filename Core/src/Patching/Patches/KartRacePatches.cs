using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;
using SLZ.Bonelab;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(GameControl_KartRace))]
    public static class KartRacePatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_KartRace.STARTRACE))]
        public static bool STARTRACE() {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                CampaignSender.SendKartRaceEvent(KartRaceEventType.START_RACE);
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_KartRace.NEWLAP))]
        public static bool NEWLAP()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                CampaignSender.SendKartRaceEvent(KartRaceEventType.NEW_LAP);
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_KartRace.RESETRACE))]
        public static bool RESETRACE()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendKartRaceEvent(KartRaceEventType.RESET_RACE);
                }
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_KartRace.ENDRACE))]
        public static bool ENDRACE()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                CampaignSender.SendKartRaceEvent(KartRaceEventType.END_RACE);
            }

            return true;
        }
    }
}
