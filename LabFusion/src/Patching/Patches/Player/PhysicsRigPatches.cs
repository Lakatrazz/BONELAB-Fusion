using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Scene;

using Il2CppSLZ.Marrow;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(PhysicsRig))]
public static class PhysicsRigPatches
{
    public static bool ForceAllowUnragdoll { get; set; } = false;

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.RagdollRig))]
    public static void RagdollRig(PhysicsRig __instance)
    {
        if (CrossSceneManager.InUnsyncedScene())
        {
            return;
        }

        if (__instance.manager.IsLocalPlayer())
        {
            using var writer = FusionWriter.Create(PhysicsRigStateData.Size);
            var data = PhysicsRigStateData.Create(PlayerIdManager.LocalSmallId, PhysicsRigStateType.RAGDOLL, true);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PhysicsRigState, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.UnRagdollRig))]
    public static bool UnRagdollRig(PhysicsRig __instance)
    {
        if (CrossSceneManager.InUnsyncedScene())
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

            using var writer = FusionWriter.Create(PhysicsRigStateData.Size);
            var data = PhysicsRigStateData.Create(PlayerIdManager.LocalSmallId, PhysicsRigStateType.RAGDOLL, false);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PhysicsRigState, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.ShutdownRig))]
    public static void ShutdownRig(PhysicsRig __instance)
    {
        if (CrossSceneManager.InUnsyncedScene())
        {
            return;
        }

        if (__instance.manager.IsLocalPlayer())
        {
            using var writer = FusionWriter.Create(PhysicsRigStateData.Size);
            var data = PhysicsRigStateData.Create(PlayerIdManager.LocalSmallId, PhysicsRigStateType.SHUTDOWN, true);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PhysicsRigState, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.TurnOnRig))]
    public static bool TurnOnRig(PhysicsRig __instance)
    {
        if (CrossSceneManager.InUnsyncedScene())
        {
            return true;
        }

        if (!__instance.manager.IsLocalPlayer())
        {
            return true;
        }

        // Check if we can unragdoll
        if (!ForceAllowUnragdoll && LocalRagdoll.RagdollLocked)
        {
            return false;
        }

        using var writer = FusionWriter.Create(PhysicsRigStateData.Size);
        var data = PhysicsRigStateData.Create(PlayerIdManager.LocalSmallId, PhysicsRigStateType.SHUTDOWN, false);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.PhysicsRigState, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.PhysicalLegs))]
    public static void PhysicalLegs(PhysicsRig __instance)
    {
        if (CrossSceneManager.InUnsyncedScene())
        {
            return;
        }

        if (__instance.manager.IsLocalPlayer())
        {
            using var writer = FusionWriter.Create(PhysicsRigStateData.Size);
            var data = PhysicsRigStateData.Create(PlayerIdManager.LocalSmallId, PhysicsRigStateType.PHYSICAL_LEGS, true);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PhysicsRigState, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.KinematicLegs))]
    public static void KinematicLegs(PhysicsRig __instance)
    {
        if (CrossSceneManager.InUnsyncedScene())
        {
            return;
        }

        if (__instance.manager.IsLocalPlayer())
        {
            using var writer = FusionWriter.Create(PhysicsRigStateData.Size);
            var data = PhysicsRigStateData.Create(PlayerIdManager.LocalSmallId, PhysicsRigStateType.PHYSICAL_LEGS, false);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PhysicsRigState, writer);
            MessageSender.SendToServer(NetworkChannel.Reliable, message);
        }
    }
}