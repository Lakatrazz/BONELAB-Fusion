using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using SLZ.Rig;
using SLZ.VRMK;

using UnityEngine;

using Avatar = SLZ.VRMK.Avatar;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(Avatar))]
    public static class AvatarPatches {
        public static bool IgnorePatches = false;

        [HarmonyPatch(nameof(Avatar.RefreshBodyMeasurements))]
        [HarmonyPatch(new Type[0])]
        [HarmonyPostfix]
        public static void RefreshBodyMeasurementsPostfix(Avatar __instance) {
            if (IgnorePatches)
                return;

            OverrideBodyMeasurements(__instance);
        }

        private static bool ValidateAvatar(Avatar avatar, out PlayerRep rep, out RigManager rm) {
            rm = avatar.GetComponentInParent<RigManager>();
            rep = null;

            // Make sure this isn't the RealHeptaRig avatar! We don't want to scale those values!
            return rm != null && PlayerRepManager.TryGetPlayerRep(rm, out rep) && avatar != rm.realHeptaRig.player && rep.avatarStats != null;
        }

        private static void OverrideBodyMeasurements(Avatar __instance) {
            try
            {
                if (NetworkInfo.HasServer && ValidateAvatar(__instance, out var rep, out var rm)) {
                    // Apply the synced avatar stats
                    rep.avatarStats.CopyTo(__instance);
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("patching Avatar.RefreshBodyMeasurements", e);
            }
        }
    }
}
