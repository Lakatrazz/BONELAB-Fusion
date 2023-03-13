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
    [HarmonyPatch(typeof(TwoButtonRemoteController))]
    public static class TwoButtonRemoteControllerPatches {
        public static bool IgnorePatches = false;

        private static bool OnJointEvent(TwoButtonRemoteController __instance, TwoButtonRemoteControllerEventType type) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer && TwoButtonRemoteControllerExtender.Cache.TryGet(__instance, out var syncable)) {
                if (syncable.IsOwner()) {
                    PowerableSender.SendTwoButtonRemoteControllerEvent(syncable.GetId(), type);
                    return true;
                }
                else {
                    return false;
                }
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TwoButtonRemoteController.DEENERGIZEJOINT))]
        public static bool DEENERGIZEJOINT(TwoButtonRemoteController __instance) {
            return OnJointEvent(__instance, TwoButtonRemoteControllerEventType.DEENERGIZEJOINT);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TwoButtonRemoteController.ENERGIZEJOINT))]
        public static bool ENERGIZEJOINT(TwoButtonRemoteController __instance)
        {
            return OnJointEvent(__instance, TwoButtonRemoteControllerEventType.ENERGIZEJOINT);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(TwoButtonRemoteController.ENERGIZEJOINTNEGATIVE))]
        public static bool ENERGIZEJOINTNEGATIVE(TwoButtonRemoteController __instance)
        {
            return OnJointEvent(__instance, TwoButtonRemoteControllerEventType.ENERGIZEJOINTNEGATIVE);
        }
    }
}
