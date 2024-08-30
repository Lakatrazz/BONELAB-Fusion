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
        try
        {
            if (NetworkInfo.HasServer && __instance.manager.IsSelf())
            {
                using var writer = FusionWriter.Create(PlayerRepRagdollData.Size);
                var data = PlayerRepRagdollData.Create(PlayerIdManager.LocalSmallId, true);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PlayerRepRagdoll, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("patching PhysicsRig.RagdollRig", e);
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
        try
        {
            if (NetworkInfo.HasServer && __instance.manager.IsSelf())
            {
                // Check if we can unragdoll
                var playerHealth = __instance.manager.health.TryCast<Player_Health>();

                if (!ForceAllowUnragdoll && playerHealth.deathIsImminent && !FusionPlayer.CanUnragdoll())
                {
                    return false;
                }

                using var writer = FusionWriter.Create(PlayerRepRagdollData.Size);
                var data = PlayerRepRagdollData.Create(PlayerIdManager.LocalSmallId, false);
                writer.Write(data);

                using var message = FusionMessage.Create(NativeMessageTag.PlayerRepRagdoll, writer);
                MessageSender.SendToServer(NetworkChannel.Reliable, message);
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("patching PhysicsRig.UnRagdollRig", e);
        }

        // Unshutdown the rig if needed
        if (__instance.shutdown)
        {
            __instance.TurnOnRig();
        }

        return true;
    }
}