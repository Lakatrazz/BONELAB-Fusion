using HarmonyLib;

using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Marrow.Messages;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.VRMK;

using LabFusion.Entities;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(PhysicsRig))]
public static class PhysicsRigPatches
{
    public static bool ForceAllowUnragdoll { get; set; } = false;

    [HarmonyPostfix]
    [HarmonyPatch(nameof(PhysicsRig.SetAvatar))]
    public static void SetAvatarPostfix(PhysicsRig __instance, Avatar avatar)
    {
        if (!NetworkManager.HasServer)
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

        TrySendPhysicsRigState(__instance, PhysicsRigStateType.Ragdoll, true);
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
            bool canUnragdoll = !LocalRagdoll.RagdollLocked || ForceAllowUnragdoll;

            if (!canUnragdoll)
            {
                return false;
            }
        }

        TrySendPhysicsRigState(__instance, PhysicsRigStateType.Ragdoll, false);

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

        TrySendPhysicsRigState(__instance, PhysicsRigStateType.Shutdown, true);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.TurnOnRig))]
    public static bool TurnOnRig(PhysicsRig __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (__instance.manager.IsLocalPlayer())
        {
            bool canUnragdoll = !LocalRagdoll.RagdollLocked || ForceAllowUnragdoll;

            if (!canUnragdoll)
            {
                return false;
            }
        }

        TrySendPhysicsRigState(__instance, PhysicsRigStateType.Shutdown, false);

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

        TrySendPhysicsRigState(__instance, PhysicsRigStateType.PhysicalLegs, true);
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(PhysicsRig.KinematicLegs))]
    public static void KinematicLegs(PhysicsRig __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        TrySendPhysicsRigState(__instance, PhysicsRigStateType.PhysicalLegs, false);
    }

    private static void TrySendPhysicsRigState(PhysicsRig physicsRig, PhysicsRigStateType type, bool enabled)
    {
        var rigManager = physicsRig.manager;

        if (!NetworkRig.Cache.TryGet(rigManager, out var networkRig))
        {
            return;
        }

        var networkEntity = networkRig.NetworkEntity;

        if (!networkEntity.IsOwner)
        {
            return;
        }

        var data = new PhysicsRigStateData()
        {
            RigReference = new(networkRig.NetworkEntity),
            Type = type,
            Enabled = enabled,
        };

        MessageRelay.RelayModule<PhysicsRigStateMessage, PhysicsRigStateData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }
}