using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Representation;

using SLZ.Interaction;
using SLZ.Props;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(Constrainer))]
    public static class ConstrainerPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Constrainer.OnTriggerGripUpdate))]
        public static bool OnTriggerGripUpdate(Hand hand) {
            if (NetworkInfo.HasServer && PlayerRep.Managers.ContainsKey(hand.manager))
                return false;

            return true;
        }
    }
}
