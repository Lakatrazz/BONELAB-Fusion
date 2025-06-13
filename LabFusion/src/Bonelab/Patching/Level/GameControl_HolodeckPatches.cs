using HarmonyLib;

using LabFusion.Network;
using LabFusion.Bonelab.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(GameControl_Holodeck))]
public static class GameControl_HolodeckPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameControl_Holodeck.SELECTMATERIAL))]
    public static void SELECTMATERIAL(GameControl_Holodeck __instance, int i)
    {
        SendHolodeckEvent(HolodeckEventType.SELECT_MATERIAL, i, false);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameControl_Holodeck.TOGGLEDOOR))]
    public static void TOGGLEDOOR(GameControl_Holodeck __instance)
    {
        SendHolodeckEvent(HolodeckEventType.TOGGLE_DOOR, 0, __instance.doorHide.activeSelf);
    }

    private static void SendHolodeckEvent(HolodeckEventType type, int selectionIndex, bool toggleValue)
    {
        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        MessageRelay.RelayModule<HolodeckEventMessage, HolodeckEventData>(new() { Type = type, SelectionIndex = selectionIndex, ToggleValue = toggleValue }, CommonMessageRoutes.ReliableToOtherClients);
    }
}
