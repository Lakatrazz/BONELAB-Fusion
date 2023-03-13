using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Syncables;
using SLZ.Bonelab;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(FunicularController))]
    public static class FunicularControllerPatches {
        public static bool IgnorePatches = false;

        private static bool OnFunicularEvent(FunicularController __instance, FunicularControllerEventType type) {
            if (IgnorePatches)
                return true;

            ushort syncId = 0;
            if (FunicularControllerExtender.Cache.TryGet(__instance, out var syncable))
                syncId = syncable.GetId();

            return QuickSender.SendServerMessage(() => {
                PowerableSender.SendFunicularControllerEvent(syncId, type);
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(FunicularController.CartGo))]
        public static bool CartGo(FunicularController __instance) {
            return OnFunicularEvent(__instance, FunicularControllerEventType.CARTGO);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(FunicularController.CartForwards))]
        public static bool CartForwards(FunicularController __instance) {
            return OnFunicularEvent(__instance, FunicularControllerEventType.CARTFORWARDS);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(FunicularController.CartBackwards))]
        public static bool CartBackwards(FunicularController __instance) {
            return OnFunicularEvent(__instance, FunicularControllerEventType.CARTBACKWARDS);
        }
    }
}
