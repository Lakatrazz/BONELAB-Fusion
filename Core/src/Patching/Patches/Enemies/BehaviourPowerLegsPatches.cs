using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Syncables;
using LabFusion.Utilities;
using PuppetMasta;

using UnityEngine;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(BehaviourPowerLegs))]
    public static class BehaviourPowerLegsPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BehaviourPowerLegs.SwitchLocoState))]
        public static bool SwitchLocoState(BehaviourPowerLegs __instance, BehaviourBaseNav.LocoState lState, float coolDown, bool forceSwitch) {
            if (BehaviourBaseNavPatches.IgnorePatches)
                return true;

            if (NetworkInfo.HasServer && BehaviourBaseNavExtender.Cache.TryGet(__instance, out var syncable))
            {
                if (syncable.IsOwner())
                {
                    EnemySender.SendLocoState(syncable, lState);
                    return true;
                }
                else
                    return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BehaviourPowerLegs.SwitchMentalState))]
        public static bool SwitchMentalState(BehaviourPowerLegs __instance, BehaviourBaseNav.MentalState mState)
        {
            if (BehaviourBaseNavPatches.IgnorePatches)
                return true;

            if (NetworkInfo.HasServer && BehaviourBaseNavExtender.Cache.TryGet(__instance, out var syncable))
            {
                if (syncable.IsOwner())
                {
                    EnemySender.SendMentalState(syncable, mState, __instance.sensors.target);
                    return true;
                }
                else
                    return false;
            }

            return true;
        }
    }
}
