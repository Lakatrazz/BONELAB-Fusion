using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Senders;

using SLZ.Bonelab;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(GameControl_Holodeck))]
    public static class GameControl_HolodeckPatches {
        public static bool IgnorePatches = false;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameControl_Holodeck.SELECTMATERIAL))]
        public static void SELECTMATERIAL(GameControl_Holodeck __instance, int i) {
            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer) {
                HolodeckSender.SendHolodeckEvent(HolodeckEventType.SELECT_MATERIAL, i);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(GameControl_Holodeck.TOGGLEDOOR))]
        public static void TOGGLEDOOR(GameControl_Holodeck __instance) {
            if (IgnorePatches)
                return;

            if (NetworkInfo.HasServer) {
                HolodeckSender.SendHolodeckEvent(HolodeckEventType.TOGGLE_DOOR, 0, __instance.doorHide.activeSelf);
            }
        }
    }
}
