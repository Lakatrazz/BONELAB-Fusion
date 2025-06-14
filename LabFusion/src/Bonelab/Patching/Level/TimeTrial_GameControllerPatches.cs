using HarmonyLib;

using LabFusion.Network;
using LabFusion.Bonelab.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(TimeTrial_GameController))]
public static class TimeTrial_GameControllerPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimeTrial_GameController.UpdateDifficulty))]
    public static bool UpdateDifficulty(int difficulty)
    {
        return SendTimeTrialEvent(TimeTrialGameControllerType.UpdateDifficulty, difficulty);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimeTrial_GameController.TIMETRIAL_PlayerStartTrigger))]
    public static bool TIMETRIAL_PlayerStartTrigger()
    {
        return SendTimeTrialEvent(TimeTrialGameControllerType.TIMETRIAL_PlayerStartTrigger, 0);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimeTrial_GameController.TIMETRIAL_PlayerEndTrigger))]
    public static bool TIMETRIAL_PlayerEndTrigger()
    {
        return SendTimeTrialEvent(TimeTrialGameControllerType.TIMETRIAL_PlayerEndTrigger, 0);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimeTrial_GameController.ProgPointKillCount))]
    public static bool ProgPointKillCount(int tCount)
    {
        return SendTimeTrialEvent(TimeTrialGameControllerType.ProgPointKillCount, tCount);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TimeTrial_GameController.SetRequiredKillCount))]
    public static bool SetRequiredKillCount(int killCount)
    {
        return SendTimeTrialEvent(TimeTrialGameControllerType.SetRequiredKillCount, killCount);
    }

    private static bool SendTimeTrialEvent(TimeTrialGameControllerType type, int value)
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

        MessageRelay.RelayModule<TimeTrialGameControllerMessage, TimeTrialGameControllerData>(new() { Type = type, Value = (byte)value }, CommonMessageRoutes.ReliableToOtherClients);
        return true;
    }
}
