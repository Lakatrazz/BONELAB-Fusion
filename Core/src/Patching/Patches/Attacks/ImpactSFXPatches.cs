using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.MarrowIntegration;
using LabFusion.Network;

using SLZ.Combat;
using SLZ.Rig;
using SLZ.SFX;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ImpactSFX))]
    public static class ImpactSFXPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ImpactSFX.BluntAttack))]
        public static bool BluntAttack(ImpactSFX __instance, float impulse, Collision c)
        {
            if (NetworkInfo.HasServer && __instance._host != null)
            {
                var properties = ImpactProperties.Cache.Get(c.gameObject);

                if (properties)
                {
                    return ImpactAttackValidator.ValidateAttack(__instance.gameObject, __instance._host, properties);
                }
            }

            return true;
        }
    }
}
