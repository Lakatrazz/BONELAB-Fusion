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
            var introEvent = DescentData.CreateIntroEvent(0, DescentIntroType.BUTTON_CONFIRM);

            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer) {
                CampaignSender.SendDescentIntro(introEvent);
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
            var introEvent = DescentData.CreateIntroEvent(gate_index, DescentIntroType.SEQUENCE);

            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer) {
                CampaignSender.SendDescentIntro(introEvent);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(GameControl_Descent.CONFIRMFORCEGRAB))]
        public static void CONFIRMFORCEGRAB()
        {
            var introEvent = DescentData.CreateIntroEvent(0, DescentIntroType.CONFIRM_FORCE_GRAB);

            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer)
            {
                CampaignSender.SendDescentIntro(introEvent);
            }
        }
    }

    [HarmonyPatch(typeof(TutorialElevator))]
    public static class ElevatorPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TutorialElevator.StartElevator))]
        public static bool StartElevator() {
            var elevatorEvent = DescentData.CreateElevatorEvent(DescentElevatorType.START_ELEVATOR);

            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                if (NetworkInfo.IsServer) {
                    CampaignSender.SendDescentElevator(elevatorEvent);
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
            var elevatorEvent = DescentData.CreateElevatorEvent(DescentElevatorType.STOP_ELEVATOR);

            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(elevatorEvent);
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
            var elevatorEvent = DescentData.CreateElevatorEvent(DescentElevatorType.SEAL_DOORS);

            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(elevatorEvent);
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
            var elevatorEvent = DescentData.CreateElevatorEvent(DescentElevatorType.START_MOVE_UPWARD);

            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(elevatorEvent);
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
            var elevatorEvent = DescentData.CreateElevatorEvent(DescentElevatorType.SLOW_UPWARD_MOVEMENT);

            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(elevatorEvent);
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
            var elevatorEvent = DescentData.CreateElevatorEvent(DescentElevatorType.OPEN_DOORS);

            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(elevatorEvent);
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
            var elevatorEvent = DescentData.CreateElevatorEvent(DescentElevatorType.CLOSE_DOORS);

            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer)
            {
                if (NetworkInfo.IsServer)
                {
                    CampaignSender.SendDescentElevator(elevatorEvent);
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
                var nooseEvent = DescentData.CreateNooseEvent(PlayerIdManager.LocalSmallId, DescentNooseType.ATTACH_NOOSE);
                CampaignSender.SendDescentNoose(nooseEvent);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(NooseBonelabIntro.NooseCut))]
        public static void NooseCut() {
            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer) {
                var nooseEvent = DescentData.CreateNooseEvent(PlayerIdManager.LocalSmallId, DescentNooseType.CUT_NOOSE);
                CampaignSender.SendDescentNoose(nooseEvent);
            }
        }
    }
}
