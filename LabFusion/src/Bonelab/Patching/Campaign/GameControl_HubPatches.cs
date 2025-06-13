using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Scene;
using LabFusion.Network;
using LabFusion.Bonelab.Messages;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(GameControl_Hub))]
public static class GameControl_HubPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Hub.ELEVATORBREAKOUT))]
    public static bool ELEVATORBREAKOUT()
    {
        return SendHubEvent(BonelabHubEventType.ELEVATOR_BREAKOUT);
    }


    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Hub.SETUPELEVATOR))]
    public static bool SETUPELEVATOR()
    {
        return SendHubEvent(BonelabHubEventType.SETUP_ELEVATOR);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Hub.OPENBWDOOR))]
    public static bool OPENBWDOOR()
    {
        return SendHubEvent(BonelabHubEventType.OPEN_BW_DOOR);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Hub.BWBOXDESTROYED))]
    public static bool BWBOXDESTROYED()
    {
        return SendHubEvent(BonelabHubEventType.BW_BOX_DESTROYED);
    }

    private static bool SendHubEvent(BonelabHubEventType type)
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

        MessageRelay.RelayModule<BonelabHubEventMessage, BonelabHubEventData>(new BonelabHubEventData() { Type = type }, CommonMessageRoutes.ReliableToOtherClients);
        return true;
    }
}
