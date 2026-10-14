using HarmonyLib;

using MarrowFusion.Bonelab.Scene;
using MarrowFusion.Bonelab.Messages;

using LabFusion.Network;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace MarrowFusion.Bonelab.Patching;

[HarmonyPatch(typeof(GameControl_Outro))]
public static class GameControl_OutroPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Outro.WarmUpJimmyArm))]
    public static bool WarmUpJimmyArm(GameControl_Outro __instance)
    {
        return SendHomeEvent(HomeEventType.WarmupJimmyArm);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Outro.ReachWindmill))]
    public static bool ReachWindmill(GameControl_Outro __instance)
    {
        return SendHomeEvent(HomeEventType.ReachWindmill);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Outro.ReachedTaxi))]
    public static bool ReachedTaxi(GameControl_Outro __instance)
    {
        return SendHomeEvent(HomeEventType.ReachedTaxi);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Outro.ArmHide))]
    public static bool ArmHide(GameControl_Outro __instance)
    {
        var result = SendHomeEvent(HomeEventType.ArmHide);

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
        return SendHomeEvent(HomeEventType.DrivingEnd);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Outro.CompleteGame))]
    public static bool CompleteGame(GameControl_Outro __instance)
    {
        return SendHomeEvent(HomeEventType.CompleteGame);
    }

    private static bool SendHomeEvent(HomeEventType type)
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

        ServerManager.SendToClientsExceptHostModule<HomeEventMessage, HomeEventData>(new HomeEventData() { Type = type }, NetworkChannel.Reliable);
        return true;
    }
}
