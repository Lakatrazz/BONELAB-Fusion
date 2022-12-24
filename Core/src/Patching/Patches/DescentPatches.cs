using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using SLZ.Bonelab;

namespace LabFusion.Patching {
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
                    DescentData.TEMP_SendElevator(DescentElevatorType.START_ELEVATOR);
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
                    DescentData.TEMP_SendElevator(DescentElevatorType.STOP_ELEVATOR);
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
                    DescentData.TEMP_SendElevator(DescentElevatorType.SEAL_DOORS);
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
                    DescentData.TEMP_SendElevator(DescentElevatorType.START_MOVE_UPWARD);
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
                    DescentData.TEMP_SendElevator(DescentElevatorType.SLOW_UPWARD_MOVEMENT);
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
                    DescentData.TEMP_SendElevator(DescentElevatorType.OPEN_DOORS);
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
                    DescentData.TEMP_SendElevator(DescentElevatorType.CLOSE_DOORS);
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
                using (var writer = FusionWriter.Create())
                {
                    using (var data = DescentNooseData.Create(PlayerIdManager.LocalSmallId, DescentNooseType.ATTACH_NOOSE))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.DescentNoose, writer))
                        {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(NooseBonelabIntro.NooseCut))]
        public static void NooseCut() {
            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer) {
                using (var writer = FusionWriter.Create())
                {
                    using (var data = DescentNooseData.Create(PlayerIdManager.LocalSmallId, DescentNooseType.CUT_NOOSE))
                    {
                        writer.Write(data);

                        using (var message = FusionMessage.Create(NativeMessageTag.DescentNoose, writer))
                        {
                            MessageSender.SendToServer(NetworkChannel.Reliable, message);
                        }
                    }
                }
            }
        }
    }
}
