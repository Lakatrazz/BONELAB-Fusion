using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Senders;
using LabFusion.SDK.Gamemodes;
using LabFusion.Extensions;
using LabFusion.Player;

using Il2CppSLZ.Marrow;

using System.Collections;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(HeadSFX))]
public static class HeadSFXPatches
{
    [HarmonyPatch(nameof(HeadSFX.RecoveryVocal))]
    [HarmonyPrefix]
    public static void RecoveryVocal(HeadSFX __instance)
    {
        // Is this our player?
        var rm = __instance._physRig.manager;

        if (NetworkInfo.HasServer && rm.IsSelf())
        {
            // Notify the server about the recovery
            PlayerSender.SendPlayerAction(PlayerActionType.RECOVERY);
        }
    }

    [HarmonyPatch(nameof(HeadSFX.DyingVocal))]
    [HarmonyPrefix]
    public static void DyingVocal(HeadSFX __instance)
    {
        // If there's no server, ignore
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var rm = __instance._physRig.manager;

        // Make sure this is the local player
        if (!rm.IsSelf())
        {
            return;
        }

        // If ragdoll on death is enabled, ragdoll the player
        if (LocalPlayer.RagdollOnDeath)
        {
            rm.physicsRig.RagdollRig();
        }

        // Notify the server about the death beginning
        if (FusionPlayer.LastAttacker.HasValue)
        {
            PlayerSender.SendPlayerAction(PlayerActionType.DYING_BY_OTHER_PLAYER, FusionPlayer.LastAttacker.Value);
        }

        PlayerSender.SendPlayerAction(PlayerActionType.DYING);
    }

    [HarmonyPatch(nameof(HeadSFX.DeathVocal))]
    [HarmonyPrefix]
    public static void DeathVocal(HeadSFX __instance)
    {
        // If there's no server, ignore
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var rm = __instance._physRig.manager;

        // Make sure this is the local player
        if (!rm.IsSelf())
        {
            return;
        }

        // Did they actually die?
        if (rm.health.alive)
        {
            return;
        }

        // If in a gamemode with auto holstering, then do it
        if (Gamemode.ActiveGamemode != null && Gamemode.ActiveGamemode.AutoHolsterOnDeath)
        {
            rm.physicsRig.leftHand.TryAutoHolsterGrip(RigData.Refs);
            rm.physicsRig.rightHand.TryAutoHolsterGrip(RigData.Refs);
        }

        // Update the spawn point
        if (FusionPlayer.TryGetSpawnPoint(out var point))
        {
            rm.checkpointPosition = point.position;
            rm.checkpointFwd = point.forward;
        }

        // Notify the server about the death
        PlayerSender.SendPlayerAction(PlayerActionType.DEATH);

        // If another player killed us, notify the server about that
        if (FusionPlayer.LastAttacker.HasValue)
        {
            PlayerSender.SendPlayerAction(PlayerActionType.DEATH_BY_OTHER_PLAYER, FusionPlayer.LastAttacker.Value);
        }

        LocalPlayer.ClearConstraints();
    }
}

[HarmonyPatch(typeof(Health))]
public static class HealthPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Health.Respawn))]
    public static void Respawn(Health __instance)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!__instance._rigManager.IsSelf())
        {
            return;
        }

        PlayerSender.SendPlayerAction(PlayerActionType.RESPAWN);

        LocalPlayer.ClearConstraints();

        // Unragdoll after respawning
        if (LocalPlayer.RagdollOnDeath)
        {
            PhysicsRigPatches.ForceAllowUnragdoll = true;

            __instance._rigManager.physicsRig.UnRagdollRig();

            PhysicsRigPatches.ForceAllowUnragdoll = false;

            // Teleport so we don't fling
            __instance._rigManager.TeleportToPose(__instance._rigManager.checkpointPosition, __instance._rigManager.checkpointFwd, true);
        }
    }
}

[HarmonyPatch(typeof(Player_Health))]
public static class PlayerHealthPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player_Health.LifeSavingDamgeDealt))]
    public static void LifeSavingDamgeDealt(Player_Health __instance)
    {
        if (__instance._rigManager.IsSelf() && LocalPlayer.RagdollOnDeath)
        {
            PhysicsRigPatches.ForceAllowUnragdoll = true;

            __instance._rigManager.physicsRig.UnRagdollRig();

            PhysicsRigPatches.ForceAllowUnragdoll = false;
        }
    }
}