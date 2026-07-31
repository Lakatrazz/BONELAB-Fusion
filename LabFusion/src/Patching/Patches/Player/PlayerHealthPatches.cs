using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Senders;
using LabFusion.SDK.Gamemodes;
using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.Preferences;
using LabFusion.Scene;
using LabFusion.Marrow.Rig;
using LabFusion.Entities;
using LabFusion.Marrow.Player;

using Il2CppSLZ.Marrow;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(HeadSFX))]
public static class HeadSFXPatches
{
    [HarmonyPatch(nameof(HeadSFX.RecoveryVocal))]
    [HarmonyPrefix]
    public static void RecoveryVocal(HeadSFX __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var rigManager = __instance._physRig.manager;

        if (!NetworkBeingManager.TryGetNetworkRig(rigManager, out var networkRig))
        {
            return;
        }

        var networkEntity = networkRig.NetworkEntity;

        if (!networkEntity.IsOwner)
        {
            return;
        }

        RigActionManager.RelayRigAction(new(networkEntity), RigActionType.Recovery);
    }

    [HarmonyPatch(nameof(HeadSFX.DyingVocal))]
    [HarmonyPrefix]
    public static void DyingVocal(HeadSFX __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var rigManager = __instance._physRig.manager;

        if (!NetworkBeingManager.TryGetNetworkRig(rigManager, out var networkRig))
        {
            return;
        }

        var networkEntity = networkRig.NetworkEntity;

        if (!networkEntity.IsOwner)
        {
            return;
        }

        RigActionManager.RelayRigAction(new(networkEntity), RigActionType.Dying);

        var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

        // If there's a NetworkPlayer, and we own it, then it can only be the local player
        if (networkPlayer != null)
        {
            OnLocalPlayerDying(rigManager);
        }
    }

    [HarmonyPatch(nameof(HeadSFX.DeathVocal))]
    [HarmonyPrefix]
    public static void DeathVocal(HeadSFX __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var rigManager = __instance._physRig.manager;

        if (rigManager.health.alive)
        {
            return;
        }

        if (!NetworkBeingManager.TryGetNetworkRig(rigManager, out var networkRig)) 
        {
            return;
        }

        var networkEntity = networkRig.NetworkEntity;

        if (!networkEntity.IsOwner)
        {
            return;
        }

        RigActionManager.RelayRigAction(new(networkEntity), RigActionType.Death);

        var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

        // If there's a NetworkPlayer, and we own it, then it can only be the local player
        if (networkPlayer != null)
        {
            OnLocalPlayerDeath(rigManager);
        }
    }

    private static void OnLocalPlayerDying(RigManager rigManager)
    {
        if (LocalPlayer.RagdollOnDeath)
        {
            LocalRagdoll.ToggleRagdoll(true);
        }

        if (FusionPlayer.LastAttacker.HasValue)
        {
            PlayerInteractManager.RelayPlayerInteraction(new(FusionPlayer.LastAttacker.Value), PlayerInteractType.KilledByOtherPlayer);
        }
    }

    private static void OnLocalPlayerDeath(RigManager rigManager)
    {
        // If in a gamemode with auto holstering, then do it
        if (GamemodeManager.IsGamemodeStarted && GamemodeManager.ActiveGamemode.AutoHolsterOnDeath)
        {
            rigManager.physicsRig.leftHand.TryAutoHolsterGrip(RigData.Refs);
            rigManager.physicsRig.rightHand.TryAutoHolsterGrip(RigData.Refs);
        }

        // Update the spawn point
        if (FusionPlayer.TryGetSpawnPoint(out var point))
        {
            rigManager.checkpointPosition = point.position;
            rigManager.checkpointFwd = point.forward;
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
        var rigManager = __instance._rigManager;

        if (rigManager.IsLocalPlayer())
        {
            LocalHealth.InvokeRespawn();
        }

        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (!NetworkBeingManager.TryGetNetworkRig(rigManager, out var networkRig))
        {
            return;
        }

        var networkEntity = networkRig.NetworkEntity;

        if (!networkEntity.IsOwner)
        {
            return;
        }

        RigActionManager.RelayRigAction(new(networkEntity), RigActionType.Respawn);

        var networkPlayer = networkEntity.GetExtender<NetworkPlayer>();

        // If there's a NetworkPlayer, and we own it, then it can only be the local player
        if (networkPlayer != null)
        {
            OnLocalPlayerRespawn();
        }
    }

    private static void OnLocalPlayerRespawn()
    {
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