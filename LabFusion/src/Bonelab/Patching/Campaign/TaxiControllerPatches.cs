using HarmonyLib;

using LabFusion.Bonelab.Messages;
using LabFusion.Network;
using LabFusion.Scene;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(TaxiController))]
public static class TaxiControllerPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(TaxiController.SplineLoopCounter))]
    public static bool SplineLoopCounter()
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

        MessageRelay.RelayModule<HomeEventMessage, HomeEventData>(new HomeEventData() { Type = HomeEventType.SPLINE_LOOP_COUNTER, SelectionNumber = 0 }, CommonMessageRoutes.ReliableToOtherClients);
        return true;
    }
}
