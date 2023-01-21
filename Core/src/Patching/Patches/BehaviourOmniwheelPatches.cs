using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Syncables;

using PuppetMasta;

using UnityEngine;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(BehaviourOmniwheel))]
    public static class BehaviourOmniwheelPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BehaviourOmniwheel.SwitchLocoState))]
        public static bool SwitchLocoState(BehaviourOmniwheel __instance, BehaviourBaseNav.LocoState lState, float coolDown, bool forceSwitch) {
            if (BehaviourBaseNavPatches.IgnorePatches)
                return true;
            
            if (NetworkInfo.HasServer && BehaviourOmniwheelExtender.Cache.TryGet(__instance, out var syncable) && !syncable.IsOwner()) {
                return false;
            }

            return true;
        }
    }
}
