using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Data;
using SLZ.Rig;

using UnityEngine;

namespace LabFusion.Patching
{
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
