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

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(StabSlash))]
    public static class StabSlashPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(StabSlash.ProcessCollision))]
        public static bool ProcessCollision(StabSlash __instance, Collision c, bool isStay)
        {
            var host = __instance._host;

            if (NetworkInfo.HasServer && host != null)
            {
                var properties = ImpactProperties.Cache.Get(c.gameObject);

                if (properties)
                {
                    bool valid = ImpactAttackValidator.ValidateAttack(__instance.gameObject, host, properties);

                    // Play the impact audio
                    if (!valid && !isStay)
                    {
                        __instance.bladeAudio.CollisionEnterSfx(c, null, host.Rb);
                    }

                    return valid;
                }
            }

            return true;
        }
    }
}
