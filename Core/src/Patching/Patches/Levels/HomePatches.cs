using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Senders;

using SLZ.Bonelab;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(TaxiController))]
    public static class TaxiControllerPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TaxiController.SplineLoopCounter))]
        public static bool SplineLoopCounter()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (!NetworkInfo.IsServer)
                    return false;
                else {
                    CampaignSender.SendHomeEvent(0, HomeEventType.SPLINE_LOOP_COUNTER);
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(GameControl_Outro))]
    public static class HomePatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Outro.WarmUpJimmyArm))]
        public static bool WarmUpJimmyArm(GameControl_Outro __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else {
                    CampaignSender.SendHomeEvent(0, HomeEventType.WARMUP_JIMMY_ARM);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Outro.ReachWindmill))]
        public static bool ReachWindmill(GameControl_Outro __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    CampaignSender.SendHomeEvent(0, HomeEventType.REACH_WINDMILL);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Outro.ReachedTaxi))]
        public static bool ReachedTaxi(GameControl_Outro __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    CampaignSender.SendHomeEvent(0, HomeEventType.REACHED_TAXI);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Outro.ArmHide))]
        public static bool ArmHide(GameControl_Outro __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    CampaignSender.SendHomeEvent(0, HomeEventType.ARM_HIDE);

                    HomeData.TeleportToJimmyFinger();
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Outro.VoidDriving))]
        public static bool VoidDriving(GameControl_Outro __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    CampaignSender.SendHomeEvent(0, HomeEventType.VOID_DRIVING);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Outro.DrivingEnd))]
        public static bool DrivingEnd(GameControl_Outro __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    CampaignSender.SendHomeEvent(0, HomeEventType.DRIVING_END);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Outro.CompleteGame))]
        public static bool CompleteGame(GameControl_Outro __instance)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    CampaignSender.SendHomeEvent(0, HomeEventType.COMPLETE_GAME);
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Outro.SequenceProgress))]
        public static bool SequenceProgress(GameControl_Outro __instance, int progress)
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (!NetworkInfo.IsServer)
                    return false;
                else
                {
                    CampaignSender.SendHomeEvent(progress, HomeEventType.SEQUENCE_PROGRESS);
                }
            }

            return true;
        }
    }
}
