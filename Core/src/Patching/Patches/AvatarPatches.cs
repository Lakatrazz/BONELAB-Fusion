using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;
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

        private static bool ValidateStats(Avatar __instance, SerializedAvatarStats stats) {
            // Make sure this is the server before validating
            if (NetworkInfo.HasServer) {
                bool isPolyblank = __instance.name.Contains(FusionAvatar.POLY_BLANK_NAME);

                // Check if this player is using a stat changer
                // We don't check polyblank as it could be a custom avatar
                if (!isPolyblank && FusionPreferences.LocalServerSettings.KickStatChangers.GetValue()) {
                    float leeway = FusionPreferences.LocalServerSettings.StatChangerLeeway.GetValue();
                    leeway = Mathf.Clamp(leeway + 1f, 1f, 11f);

                    // Health
                    if (stats.vitality > __instance.vitality * 2f * leeway)
                        return false;

                    // Speed
                    if (stats.speed > __instance.speed * 4f * leeway)
                        return false;

                    // Agility
                    if (stats.agility > __instance.agility * 4f * leeway)
                        return false;

                    // Strength
                    if (stats.strengthUpper > __instance.strengthUpper * 5f * leeway)
                        return false;

                    // Mass
                    if (stats.massTotal > __instance.massTotal * 3f * leeway)
                        return false;
                }
            }
            
            return true;
        }

        private static void OverrideBodyMeasurements(Avatar __instance) {
            try
            {
                if (NetworkInfo.HasServer && ValidateAvatar(__instance, out var rep, out var rm)) {
                    var newStats = rep.avatarStats;

                    // Make sure the stats are valid before applying them
                    if (!ValidateStats(__instance, newStats)) {
                        ConnectionSender.SendDisconnect(rep.PlayerId, "Stat changers are not allowed on this server. Your stats appear to be modified.");
                        return;
                    }

                    // Apply the synced avatar stats
                    newStats.CopyTo(__instance);
                }
            }
            catch (Exception e)
            {
                FusionLogger.LogException("patching Avatar.RefreshBodyMeasurements", e);
            }
        }
    }
}
