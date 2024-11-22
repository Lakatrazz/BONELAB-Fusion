using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(PhysicsRig))]
public static class PhysicsRigPatches
{
    public static bool ForceAllowUnragdoll = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.RagdollRig))]
    public static bool RagdollRig(PhysicsRig __instance)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        if (__instance.manager.IsLocalPlayer())
        {
            using var writer = FusionWriter.Create(PlayerRepRagdollData.Size);
            var data = PlayerRepRagdollData.Create(PlayerIdManager.LocalSmallId, true);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PlayerRepRagdoll, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        // If not already shutdown, shutdown the rig
        // This is required for patch 4 ragdolling
        // Strangely, the bodyState system doesn't appear to properly call this
        if (!__instance.shutdown)
        {
            __instance.ShutdownRig();
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.UnRagdollRig))]
    public static bool UnRagdollRig(PhysicsRig __instance)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        if (__instance.manager.IsLocalPlayer())
        {
            // Check if we can unragdoll
            if (!ForceAllowUnragdoll && LocalRagdoll.RagdollLocked)
            {
                return false;
            }

            using var writer = FusionWriter.Create(PlayerRepRagdollData.Size);
            var data = PlayerRepRagdollData.Create(PlayerIdManager.LocalSmallId, false);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PlayerRepRagdoll, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        // Unshutdown the rig if needed
        if (__instance.shutdown)
        {
            __instance.TurnOnRig();
        }

        return true;
    }
}