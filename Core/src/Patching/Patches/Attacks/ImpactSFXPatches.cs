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

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(ImpactSFX))]
    public static class ImpactSFXPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ImpactSFX.BluntAttack))]
        public static bool BluntAttack(ImpactSFX __instance, float impulse, Collision c) {
            if (NetworkInfo.HasServer && __instance._host != null) {
                var properties = ImpactProperties.Cache.Get(c.gameObject);

                if (properties) {
                    var physRig = properties.GetComponentInParent<PhysicsRig>();

                    // Was a player damaged? Make sure another player is holding the weapon
                    if (physRig != null) {
                        // Check if we can force enable
                        if (AlwaysAllowImpactDamage.Cache.ContainsSource(__instance.gameObject))
                            return true;

                        var host = __instance._host;

                        foreach (var hand in host._hands) {
                            if (hand.manager != physRig.manager)
                                return true;
                        }
                        
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
