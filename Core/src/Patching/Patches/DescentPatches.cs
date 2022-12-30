using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;

using SLZ.Bonelab;
using SLZ.UI;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(Control_UI_BodyMeasurements))]
    public static class Control_UI_BodyMeasurementsPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Control_UI_BodyMeasurements.BUTTON_CONFIRM))]
        public static void BUTTON_CONFIRM()
        {
            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer)
            {
                CampaignSender.SendDescentIntro(0, DescentIntroType.BUTTON_CONFIRM);
            }
        }
    }

    [HarmonyPatch(typeof(GameControl_Descent))]
    public static class GameControl_DescentPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Descent.SEQUENCE))]
        public static void SEQUENCE(int gate_index)
        {
            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer) {
                CampaignSender.SendDescentIntro(gate_index, DescentIntroType.SEQUENCE);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Descent.CONFIRMFORCEGRAB))]
        public static void CONFIRMFORCEGRAB()
        {
            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer)
            {
                CampaignSender.SendDescentIntro(0, DescentIntroType.CONFIRM_FORCE_GRAB);
            }
        }
    }

    [HarmonyPatch(typeof(TutorialElevator))]
    public static class ElevatorPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialElevator.StartElevator))]
        public static bool StartElevator() {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (NetworkInfo.IsServer) {
                    CampaignSender.SendDescentElevator(DescentElevatorType.START_ELEVATOR);
                    return true;
                }
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialElevator.StopDoorRoutine))]
        public static bool StopDoorRoutine()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(DescentElevatorType.STOP_ELEVATOR);
                    return true;
                }
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialElevator.SealDoors))]
        public static bool SealDoors()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(DescentElevatorType.SEAL_DOORS);
                    return true;
                }
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialElevator.StartMoveUpward))]
        public static bool StartMoveUpward()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(DescentElevatorType.START_MOVE_UPWARD);
                    return true;
                }
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialElevator.SlowUpwardMovement))]
        public static bool SlowUpwardMovement()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(DescentElevatorType.SLOW_UPWARD_MOVEMENT);
                    return true;
                }
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialElevator.OpenDoors))]
        public static bool OpenDoors()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(DescentElevatorType.OPEN_DOORS);
                    return true;
                }
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialElevator.CloseDoors))]
        public static bool CloseDoors()
        {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(DescentElevatorType.CLOSE_DOORS);
                    return true;
                }
                else
                    return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(NooseBonelabIntro))]
    public static class NoosePatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(NooseBonelabIntro.AttachNeck))]
        public static void AttachNeck() {
            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer) {
                CampaignSender.SendDescentNoose(DescentNooseType.ATTACH_NOOSE);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(NooseBonelabIntro.NooseCut))]
        public static void NooseCut() {
            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer) {
                CampaignSender.SendDescentNoose(DescentNooseType.CUT_NOOSE);
            }
        }
    }
}
