using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;

using SLZ.Interaction;
using SLZ.Props;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(AvatarDice))]
    public static class AvatarDicePatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(AvatarDice.OnHandAttached))]
        public static void OnHandAttached(AvatarDice __instance, InteractableHost host, Hand hand) {
            // Force update the manager
            if (NetworkInfo.HasServer) {
                __instance.rigManager = hand.manager;
            }
        }
    }
}
