using HarmonyLib;

using LabFusion.Bonelab.Scene;
using LabFusion.Network;
using LabFusion.Bonelab.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(GameControl_Outro))]
public static class GameControl_OutroPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Outro.WarmUpJimmyArm))]
    public static bool WarmUpJimmyArm(GameControl_Outro __instance)
    {
        return SendHomeEvent(0, HomeEventType.WARMUP_JIMMY_ARM);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Outro.ReachWindmill))]
    public static bool ReachWindmill(GameControl_Outro __instance)
    {
        return SendHomeEvent(0, HomeEventType.REACH_WINDMILL);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Outro.ReachedTaxi))]
    public static bool ReachedTaxi(GameControl_Outro __instance)
    {
        return SendHomeEvent(0, HomeEventType.REACHED_TAXI);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Outro.ArmHide))]
    public static bool ArmHide(GameControl_Outro __instance)
    {
        var result = SendHomeEvent(0, HomeEventType.ARM_HIDE);

        if (result && NetworkSceneManager.IsLevelNetworked)
        {
            HomeEventHandler.TeleportToJimmyFinger();
        }

        return result;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Outro.DrivingEnd))]
    public static bool DrivingEnd(GameControl_Outro __instance)
    {
        return SendHomeEvent(0, HomeEventType.DRIVING_END);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Outro.CompleteGame))]
    public static bool CompleteGame(GameControl_Outro __instance)
    {
        return SendHomeEvent(0, HomeEventType.COMPLETE_GAME);
    }

    private static bool SendHomeEvent(byte selectionNumber, HomeEventType type)
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

        MessageRelay.RelayModule<HomeEventMessage, HomeEventData>(new HomeEventData() { Type = type, SelectionNumber = selectionNumber }, CommonMessageRoutes.ReliableToOtherClients);
        return true;
    }
}
