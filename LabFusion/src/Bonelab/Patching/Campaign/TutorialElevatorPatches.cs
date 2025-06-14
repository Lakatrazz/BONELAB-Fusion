using HarmonyLib;

using LabFusion.Bonelab.Scene;
using LabFusion.Network;
using LabFusion.Bonelab.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(TutorialElevator))]
public static class TutorialElevatorPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TutorialElevator.StartElevator))]
    public static bool StartElevator()
    {
        return SendElevatorEvent(DescentElevatorType.START_ELEVATOR);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TutorialElevator.StopDoorRoutine))]
    public static bool StopDoorRoutine()
    {
        return SendElevatorEvent(DescentElevatorType.STOP_ELEVATOR);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TutorialElevator.SealDoors))]
    public static bool SealDoors()
    {
        return SendElevatorEvent(DescentElevatorType.SEAL_DOORS);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TutorialElevator.StartMoveUpward))]
    public static bool StartMoveUpward()
    {
        return SendElevatorEvent(DescentElevatorType.START_MOVE_UPWARD);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TutorialElevator.SlowUpwardMovement))]
    public static bool SlowUpwardMovement()
    {
        return SendElevatorEvent(DescentElevatorType.SLOW_UPWARD_MOVEMENT);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TutorialElevator.OpenDoors))]
    public static bool OpenDoors()
    {
        return SendElevatorEvent(DescentElevatorType.OPEN_DOORS);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TutorialElevator.CloseDoors))]
    public static bool CloseDoors()
    {
        return SendElevatorEvent(DescentElevatorType.CLOSE_DOORS);
    }

    private static bool SendElevatorEvent(DescentElevatorType type)
    {
        if (IgnorePatches)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (!NetworkSceneManager.IsLevelHost)
        {
            return false;
        }

        var elevatorEvent = DescentEventHandler.CreateElevatorEvent(type);

        MessageRelay.RelayModule<DescentElevatorMessage, DescentElevatorData>(new DescentElevatorData() { Type = elevatorEvent.Type }, CommonMessageRoutes.ReliableToOtherClients);
        return true;
    }
}
