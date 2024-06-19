using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Senders;
using LabFusion.SDK.Gamemodes;
using LabFusion.Extensions;

using Il2CppSLZ.SFX;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Player;

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

        // If the player has ragdoll on death enabled, ragdoll them
        var health = rm.health;
        if (health._testRagdollOnDeath)
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
        if (!rm.health.alive)
        {
            // If in a gamemode with auto holstering, then do it
            if (Gamemode.ActiveGamemode != null && Gamemode.ActiveGamemode.AutoHolsterOnDeath)
            {
                rm.physicsRig.leftHand.TryAutoHolsterGrip(RigData.RigReferences);
                rm.physicsRig.rightHand.TryAutoHolsterGrip(RigData.RigReferences);
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
                PlayerSender.SendPlayerAction(PlayerActionType.DEATH_BY_OTHER_PLAYER, FusionPlayer.LastAttacker.Value);
        }
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
    }
}

[HarmonyPatch(typeof(Player_Health))]
public static class PlayerHealthPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player_Health.LifeSavingDamgeDealt))]
    public static void LifeSavingDamgeDealt(Player_Health __instance)
    {
        if (__instance._rigManager.IsSelf() && __instance._testRagdollOnDeath)
        {
            PhysicsRigPatches.ForceAllowUnragdoll = true;

            __instance._rigManager.physicsRig.UnRagdollRig();

            PhysicsRigPatches.ForceAllowUnragdoll = false;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(Player_Health.TAKEDAMAGE))]
    public static void TAKEDAMAGEPrefix(Player_Health __instance, float damage)
    {
        if (__instance.healthMode == Health.HealthMode.Invincible && __instance._testRagdollOnDeath)
        {
            __instance._testRagdollOnDeath = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(Player_Health.TAKEDAMAGE))]
    public static void TAKEDAMAGEPostfix(Player_Health __instance, float damage)
    {
        if (__instance._rigManager.IsSelf() && __instance._testRagdollOnDeath && !__instance.alive)
        {
            PhysicsRigPatches.ForceAllowUnragdoll = true;

            __instance._rigManager.physicsRig.UnRagdollRig();

            PhysicsRigPatches.ForceAllowUnragdoll = false;
        }
    }
}