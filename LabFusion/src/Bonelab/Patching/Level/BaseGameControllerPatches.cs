using HarmonyLib;

using LabFusion.Network;
using LabFusion.Bonelab.Messages;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(BaseGameController))]
public static class BaseGameControllerPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BaseGameController.BeginSession))]
    public static bool BeginSession()
    {
        return SendGameControllerEvent(BaseGameControllerType.BeginSession);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(BaseGameController.EndSession))]
    public static bool EndSession()
    {
        return SendGameControllerEvent(BaseGameControllerType.EndSession);
    }

    private static bool SendGameControllerEvent(BaseGameControllerType type)
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

        MessageRelay.RelayModule<BaseGameControllerMessage, BaseGameControllerData>(new() { Type = type }, CommonMessageRoutes.ReliableToOtherClients);
        return true;
    }
}
