using HarmonyLib;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow;

using UnityEngine;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(PhysicsRig))]
public static class PhysicsRigPatches
{
    public static bool ForceAllowUnragdoll = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.TeleportToPose))]
    public static void TeleportToPosePrefix(PhysicsRig __instance, ref Vector3 __state)
    {
        __state = __instance.feet.transform.position;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PhysicsRig.TeleportToPose))]
    public static void TeleportToPosePostfix(PhysicsRig __instance, ref Vector3 __state)
    {
        var kneeTransform = __instance.knee.transform;
        var feetTransform = __instance.feet.transform;

        var localFeet = kneeTransform.InverseTransformPoint(feetTransform.position);
        kneeTransform.localRotation = QuaternionExtensions.identity;
        feetTransform.position = kneeTransform.TransformPoint(localFeet);

        __instance.transform.position += __state - feetTransform.position;
    }

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