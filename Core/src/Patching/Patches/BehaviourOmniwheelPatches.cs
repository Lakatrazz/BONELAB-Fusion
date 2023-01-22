using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Syncables;

using PuppetMasta;

using UnityEngine;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(BehaviourOmniwheel))]
    public static class BehaviourOmniwheelPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(BehaviourOmniwheel.SwitchLocoState))]
        public static bool SwitchLocoState(BehaviourOmniwheel __instance, BehaviourBaseNav.LocoState lState, float coolDown, bool forceSwitch)
        {
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
        [HarmonyPatch(nameof(BehaviourOmniwheel.SwitchMentalState))]
        public static bool SwitchMentalState(BehaviourOmniwheel __instance, BehaviourBaseNav.MentalState mState)
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
