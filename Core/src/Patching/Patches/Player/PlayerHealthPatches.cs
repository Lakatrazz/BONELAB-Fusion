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
            if (NetworkInfo.HasServer && rm == RigData.RigReferences.RigManager && !rm.health.alive) {
                // If the player is ragdoll on death, reset them
                // This prevents them spawning in the ground
                if (rm.health._testRagdollOnDeath) {
                    rm.physicsRig.UnRagdollRig();
                    rm.physicsRig.ResetHands(Handedness.BOTH);
                    rm.physicsRig.TeleportToPose();
                }

                // Notify the server about the death
                PlayerSender.SendPlayerAction(PlayerActionType.DEATH);
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
            if (__instance._testRagdollOnDeath) {
                __instance._rigManager.physicsRig.UnRagdollRig();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Player_Health.TAKEDAMAGE))]
        public static void TAKEDAMAGE(Player_Health __instance, float damage) {
            if (__instance.healthMode == Health.HealthMode.Invincible && __instance._testRagdollOnDeath)
                __instance._testRagdollOnDeath = false;
        }
    }
}
