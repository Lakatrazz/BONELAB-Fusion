using HarmonyLib;

using LabFusion.Bonelab.Scene;
using LabFusion.Network;
using LabFusion.Bonelab.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(GameControl_Descent))]
public static class GameControl_DescentPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Descent.SEQUENCE))]
    public static void SEQUENCE(int gate_index)
    {
        var introEvent = DescentEventHandler.CreateIntroEvent(gate_index, DescentIntroType.SEQUENCE);

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

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_Descent.CONFIRMFORCEGRAB))]
    public static void CONFIRMFORCEGRAB()
    {
        var introEvent = DescentEventHandler.CreateIntroEvent(0, DescentIntroType.CONFIRM_FORCE_GRAB);

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
