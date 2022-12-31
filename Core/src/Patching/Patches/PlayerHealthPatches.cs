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

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(HeadSFX))]
    public static class HeadSFXPatches {
        [HarmonyPatch(nameof(HeadSFX.DeathVocal))]
        [HarmonyPrefix]
        public static void DeathVocal(HeadSFX __instance) {
            // Did this player actually die? + do we need to reset the rig so that it respawns properly from a ragdoll?
            var rm = __instance.physRig.manager;
            if (NetworkInfo.HasServer && rm.health._testRagdollOnDeath && !rm.health.alive) {
                rm.physicsRig.UnRagdollRig();
                rm.physicsRig.ResetHands(Handedness.BOTH);
                rm.physicsRig.TeleportToPose();
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
    }
}
