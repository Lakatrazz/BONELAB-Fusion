using HarmonyLib;

using LabFusion.Network;
using LabFusion.Marrow.Extenders;
using LabFusion.Scene;
using LabFusion.Marrow.Messages;

using Il2CppSLZ.Marrow;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(ConstraintTracker))]
public static class ConstraintTrackerPatches
{
    public static bool IgnorePatches { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ConstraintTracker.DeleteConstraint))]
    public static bool DeleteConstraint(ConstraintTracker __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (IgnorePatches || !__instance.isActiveAndEnabled)
        {
            return true;
        }

        var constraintEntity = NetworkConstraint.Cache.Get(__instance);

        if (constraintEntity == null)
        {
            return true;
        }

        var data = new ConstraintDeleteData()
        {
            ConstraintID = constraintEntity.ID,
        };

        MessageRelay.RelayModule<ConstraintDeleteMessage, ConstraintDeleteData>(data, CommonMessageRoutes.ReliableToClients);

        // Return false, because constraints are deleted server side
        return false;
    }
}