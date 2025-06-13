using HarmonyLib;

using LabFusion.Network;
using LabFusion.Bonelab.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(GameControl_MagmaGate))]
public static class GameControl_MagmaGatePatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_MagmaGate.LoseSequence))]
    public static bool LoseSequence(GameControl_MagmaGate __instance)
    {
        return SendMagmaGateEvent(MagmaGateEventType.LOSE_SEQUENCE);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameControl_MagmaGate.DoorDissolve))]
    public static bool DoorDissolve(GameControl_MagmaGate __instance)
    {
        return SendMagmaGateEvent(MagmaGateEventType.DOOR_DISSOLVE);
    }

    private static bool SendMagmaGateEvent(MagmaGateEventType type)
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

        MessageRelay.RelayModule<MagmaGateEventMessage, MagmaGateEventData>(new MagmaGateEventData() { Type = type }, CommonMessageRoutes.ReliableToOtherClients);
        return true;
    }
}
