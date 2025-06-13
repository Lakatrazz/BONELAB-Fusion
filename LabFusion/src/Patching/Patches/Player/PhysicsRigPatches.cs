using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Scene;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(PhysicsRig))]
public static class PhysicsRigPatches
{
    public static bool ForceAllowUnragdoll { get; set; } = false;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PhysicsRig.SetAvatar))]
    public static void SetAvatarPostfix(PhysicsRig __instance, Avatar avatar)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (__instance._impactProperties == null)
        {
            return;
        }

        // PhysicsRig sets surfaceData but not cachedSurfaceData
        // Why are these even separate variables? WHO KNOWS!
        foreach (var properties in __instance._impactProperties)
        {
            properties._cachedSurfaceData = properties.surfaceData;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.RagdollRig))]
    public static void RagdollRig(PhysicsRig __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (__instance.manager.IsLocalPlayer())
        {
            var data = PhysicsRigStateData.Create(PlayerIDManager.LocalSmallID, PhysicsRigStateType.RAGDOLL, true);

            MessageRelay.RelayNative(data, NativeMessageTag.PhysicsRigState, CommonMessageRoutes.ReliableToOtherClients);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.UnRagdollRig))]
    public static bool UnRagdollRig(PhysicsRig __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
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

            var data = PhysicsRigStateData.Create(PlayerIDManager.LocalSmallID, PhysicsRigStateType.RAGDOLL, false);

            MessageRelay.RelayNative(data, NativeMessageTag.PhysicsRigState, CommonMessageRoutes.ReliableToOtherClients);
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.ShutdownRig))]
    public static void ShutdownRig(PhysicsRig __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (__instance.manager.IsLocalPlayer())
        {
            var data = PhysicsRigStateData.Create(PlayerIDManager.LocalSmallID, PhysicsRigStateType.SHUTDOWN, true);

            MessageRelay.RelayNative(data, NativeMessageTag.PhysicsRigState, CommonMessageRoutes.ReliableToOtherClients);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.TurnOnRig))]
    public static bool TurnOnRig(PhysicsRig __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
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

        var data = PhysicsRigStateData.Create(PlayerIDManager.LocalSmallID, PhysicsRigStateType.SHUTDOWN, false);

        MessageRelay.RelayNative(data, NativeMessageTag.PhysicsRigState, CommonMessageRoutes.ReliableToOtherClients);

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.PhysicalLegs))]
    public static void PhysicalLegs(PhysicsRig __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (__instance.manager.IsLocalPlayer())
        {
            var data = PhysicsRigStateData.Create(PlayerIDManager.LocalSmallID, PhysicsRigStateType.PHYSICAL_LEGS, true);

            MessageRelay.RelayNative(data, NativeMessageTag.PhysicsRigState, CommonMessageRoutes.ReliableToOtherClients);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.KinematicLegs))]
    public static void KinematicLegs(PhysicsRig __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (__instance.manager.IsLocalPlayer())
        {
            var data = PhysicsRigStateData.Create(PlayerIDManager.LocalSmallID, PhysicsRigStateType.PHYSICAL_LEGS, false);

            MessageRelay.RelayNative(data, NativeMessageTag.PhysicsRigState, CommonMessageRoutes.ReliableToOtherClients);
        }
    }
}