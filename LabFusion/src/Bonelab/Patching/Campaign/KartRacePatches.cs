using HarmonyLib;

using LabFusion.Network;
using LabFusion.Bonelab.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(GameControl_KartRace))]
public static class KartRacePatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_KartRace.STARTRACE))]
    public static void STARTRACE()
    {
        if (IgnorePatches)
        {
            return;
        }

        if (NetworkSceneManager.IsLevelNetworked)
        {
            MessageRelay.RelayModule<KartRaceEventMessage, KartRaceEventData>(new KartRaceEventData() { Type = KartRaceEventType.START_RACE }, CommonMessageRoutes.ReliableToOtherClients);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_KartRace.NEWLAP))]
    public static void NEWLAP()
    {
        if (IgnorePatches)
        {
            return;
        }

        if (NetworkSceneManager.IsLevelNetworked)
        {
            MessageRelay.RelayModule<KartRaceEventMessage, KartRaceEventData>(new KartRaceEventData() { Type = KartRaceEventType.NEW_LAP }, CommonMessageRoutes.ReliableToOtherClients);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_KartRace.RESETRACE))]
    public static bool RESETRACE()
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

        MessageRelay.RelayModule<KartRaceEventMessage, KartRaceEventData>(new KartRaceEventData() { Type = KartRaceEventType.RESET_RACE }, CommonMessageRoutes.ReliableToOtherClients);

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_KartRace.ENDRACE))]
    public static void ENDRACE()
    {
        if (IgnorePatches)
        {
            return;
        }

        if (NetworkSceneManager.IsLevelNetworked)
        {
            MessageRelay.RelayModule<KartRaceEventMessage, KartRaceEventData>(new KartRaceEventData() { Type = KartRaceEventType.END_RACE }, CommonMessageRoutes.ReliableToOtherClients);
        }
    }
}
