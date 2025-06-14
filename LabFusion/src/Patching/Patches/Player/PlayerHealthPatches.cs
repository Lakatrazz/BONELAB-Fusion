using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Senders;
using LabFusion.SDK.Gamemodes;
using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.Preferences;

using Il2CppSLZ.Marrow;

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

        if (NetworkInfo.HasServer && rm.IsLocalPlayer())
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
        if (!rm.IsLocalPlayer())
        {
            return;
        }

        // If ragdoll on death is enabled, ragdoll the player
        if (LocalPlayer.RagdollOnDeath)
        {
            LocalRagdoll.ToggleRagdoll(true);
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
        if (!rm.IsLocalPlayer())
        {
            return;
        }

        // Did they actually die?
        if (rm.health.alive)
        {
            return;
        }

        // If in a gamemode with auto holstering, then do it
        if (GamemodeManager.IsGamemodeStarted && GamemodeManager.ActiveGamemode.AutoHolsterOnDeath)
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
        if (!__instance._rigManager.IsLocalPlayer())
        {
            return;
        }

        LocalHealth.InvokeRespawn();

        if (!NetworkInfo.HasServer)
        {
            return;
        }

        PlayerSender.SendPlayerAction(PlayerActionType.RESPAWN);

        LocalPlayer.ClearConstraints();

        // Unragdoll after respawning
        if (LocalPlayer.RagdollOnDeath)
        {
            LocalRagdoll.ToggleRagdoll(false);

            // Teleport so we don't fling
            LocalPlayer.TeleportToCheckpoint();
        }
    }
}

[HarmonyPatch(typeof(Player_Health))]
public static class PlayerHealthPatches
{
    // Teleport AFTER ApplyKillDamage so that the player teleports properly and not extremely far away
    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player_Health.ApplyKillDamage))]
    public static void ApplyKillDamagePostfix(Player_Health __instance)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!__instance._rigManager.IsLocalPlayer())
        {
            return;
        }

        if (__instance.healthMode == Health.HealthMode.Mortal)
        {
            return;
        }

        LocalPlayer.TeleportToCheckpoint();
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player_Health.Dying))]
    public static void Dying(Player_Health __instance)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        if (!__instance._rigManager.IsLocalPlayer())
        {
            return;
        }

        if (CommonPreferences.Knockout && CommonPreferences.Mortality && __instance.healthMode == Health.HealthMode.Invincible && !LocalHealth.MortalityOverride.HasValue)
        {
            LocalRagdoll.Knockout(LobbyInfoManager.LobbyInfo.KnockoutLength);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player_Health.LifeSavingDamgeDealt))]
    public static void LifeSavingDamgeDealt(Player_Health __instance)
    {
        if (__instance._rigManager.IsLocalPlayer() && LocalPlayer.RagdollOnDeath)
        {
            LocalRagdoll.ToggleRagdoll(false);
        }
    }
}