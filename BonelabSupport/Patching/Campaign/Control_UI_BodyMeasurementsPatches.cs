using HarmonyLib;

using MarrowFusion.Bonelab.Scene;
using LabFusion.Network;
using MarrowFusion.Bonelab.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace MarrowFusion.Bonelab.Patching;

[HarmonyPatch(typeof(Control_UI_BodyMeasurements))]
public static class Control_UI_BodyMeasurementsPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Control_UI_BodyMeasurements.BUTTON_CONFIRM))]
    public static void BUTTON_CONFIRM()
    {
        var introEvent = DescentEventHandler.CreateIntroEvent(0, DescentIntroType.BUTTON_CONFIRM);

        if (IgnorePatches)
        {
            return;
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        MessageRelay.RelayModule<DescentIntroMessage, DescentIntroData>(new DescentIntroData() { Type = introEvent.Type, SelectionNumber = (byte)introEvent.SelectionNumber }, CommonMessageRoutes.ReliableToOtherClients);
    }
}
