using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;

using SLZ.Bonelab;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(TimeTrial_GameController))]
    public static class TimeTrial_GameControllerPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TimeTrial_GameController.UpdateDifficulty))]
        public static bool UpdateDifficulty(int difficulty)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (!NetworkInfo.IsServer)
                    return false;
                else {
                    TrialSender.SendTimeTrialEvent(TimeTrialGameControllerType.UpdateDifficulty, difficulty);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TimeTrial_GameController.TIMETRIAL_PlayerStartTrigger))]
        public static bool TIMETRIAL_PlayerStartTrigger()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    TrialSender.SendTimeTrialEvent(TimeTrialGameControllerType.TIMETRIAL_PlayerStartTrigger, 0);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TimeTrial_GameController.TIMETRIAL_PlayerEndTrigger))]
        public static bool TIMETRIAL_PlayerEndTrigger()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    TrialSender.SendTimeTrialEvent(TimeTrialGameControllerType.TIMETRIAL_PlayerEndTrigger, 0);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TimeTrial_GameController.ProgPointKillCount))]
        public static bool ProgPointKillCount(int tCount)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    TrialSender.SendTimeTrialEvent(TimeTrialGameControllerType.ProgPointKillCount, tCount);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TimeTrial_GameController.SetRequiredKillCount))]
        public static bool SetRequiredKillCount(int killCount)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    TrialSender.SendTimeTrialEvent(TimeTrialGameControllerType.SetRequiredKillCount, killCount);
                }
            }

            return true;
        }
    }
}
