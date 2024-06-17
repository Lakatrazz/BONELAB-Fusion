using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Entities;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(ConstraintTracker))]
public static class ConstraintTrackerPatches
{
    public static bool IgnorePatches = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(ConstraintTracker.DeleteConstraint))]
    public static bool DeleteConstraint(ConstraintTracker __instance)
    {
        if (!NetworkInfo.HasServer)
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

        using var writer = FusionWriter.Create(ConstraintDeleteData.Size);
        var data = ConstraintDeleteData.Create(PlayerIdManager.LocalSmallId, constraintEntity.Id);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.ConstraintDelete, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);

        // Return false, because constraints are deleted server side
        return false;
    }
}