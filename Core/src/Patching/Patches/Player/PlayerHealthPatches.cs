using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;

using SLZ.Rig;
using SLZ.SFX;
using SLZ;

using UnityEngine;
using LabFusion.Senders;
using LabFusion.Representation;
using LabFusion.SDK.Gamemodes;
using LabFusion.Extensions;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(HeadSFX))]
    public static class HeadSFXPatches {
        [HarmonyPatch(nameof(HeadSFX.RecoveryVocal))]
        [HarmonyPrefix]
        public static void RecoveryVocal(HeadSFX __instance) {
            // Is this our player?
            var rm = __instance.physRig.manager;
            if (NetworkInfo.HasServer && rm == RigData.RigReferences.RigManager) {
                // Notify the server about the recovery
                PlayerSender.SendPlayerAction(PlayerActionType.RECOVERY);
            }
        }

        [HarmonyPatch(nameof(HeadSFX.DyingVocal))]
        [HarmonyPrefix]
        public static void DyingVocal(HeadSFX __instance) {
            // Is this our player?
            var rm = __instance.physRig.manager;
            if (NetworkInfo.HasServer && rm == RigData.RigReferences.RigManager) {
                // Notify the server about the death beginning
                PlayerSender.SendPlayerAction(PlayerActionType.DYING);
            }
        }

        [HarmonyPatch(nameof(HeadSFX.DeathVocal))]
        [HarmonyPrefix]
        public static void DeathVocal(HeadSFX __instance) {
            // Is this our player? Did they actually die?
            var rm = __instance.physRig.manager;
            if (NetworkInfo.HasServer && rm.IsLocalPlayer() && !rm.health.alive) {
                // If in a gamemode with auto holstering, then do it
                if (Gamemode.ActiveGamemode != null && Gamemode.ActiveGamemode.AutoHolsterOnDeath) {
                    rm.physicsRig.leftHand.TryAutoHolsterGrip(RigData.RigReferences);
                    rm.physicsRig.rightHand.TryAutoHolsterGrip(RigData.RigReferences);
                }

                // If the player is ragdoll on death, reset them
                // This prevents them spawning in the ground
                if (rm.health._testRagdollOnDeath) {
                    PhysicsRigPatches.ForceAllowUnragdoll = true;
                    rm.physicsRig.UnRagdollRig();
                    rm.physicsRig.ResetHands(Handedness.BOTH);
                    rm.physicsRig.TeleportToPose();
                    PhysicsRigPatches.ForceAllowUnragdoll = false;
                }

                // Update the spawn point
                if (FusionPlayer.TryGetSpawnPoint(out var point)) {
                    rm.bodyVitals.checkpointPosition = point.position;
                    rm.bodyVitals.checkpointFwd = point.forward;
                }

                // Notify the server about the death
                PlayerSender.SendPlayerAction(PlayerActionType.DEATH);

                // If another player killed us, notify the server about that
                if (FusionPlayer.LastAttacker.HasValue)
                    PlayerSender.SendPlayerAction(PlayerActionType.DEATH_BY_OTHER_PLAYER, FusionPlayer.LastAttacker.Value);
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
            if (__instance._rigManager == RigData.RigReferences.RigManager && __instance._testRagdollOnDeath) {
                PhysicsRigPatches.ForceAllowUnragdoll = true;
                __instance._rigManager.physicsRig.UnRagdollRig();
                PhysicsRigPatches.ForceAllowUnragdoll = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player_Health.TAKEDAMAGE))]
        public static void TAKEDAMAGEPrefix(Player_Health __instance, float damage) {
            if (__instance.healthMode == Health.HealthMode.Invincible && __instance._testRagdollOnDeath)
                __instance._testRagdollOnDeath = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Player_Health.TAKEDAMAGE))]
        public static void TAKEDAMAGEPostfix(Player_Health __instance, float damage) {
            if (__instance._rigManager == RigData.RigReferences.RigManager && __instance._testRagdollOnDeath && !__instance.alive) {
                PhysicsRigPatches.ForceAllowUnragdoll = true;
                __instance._rigManager.physicsRig.UnRagdollRig();
                PhysicsRigPatches.ForceAllowUnragdoll = false;
            }
        }
    }
}
