using System.Collections;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;
using LabFusion.Utilities;

using MelonLoader;

using Il2CppSLZ.Rig;

using Avatar = Il2CppSLZ.VRMK.Avatar;
using Il2CppSLZ.VRMK;
using LabFusion.Entities;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Avatar))]
    public static class AvatarPatches
    {
        public static bool IgnorePatches = false;

        [HarmonyPatch(nameof(Avatar.RefreshBodyMeasurements))]
        [HarmonyPatch(new Type[0])]
        [HarmonyPostfix]
        public static void RefreshBodyMeasurementsPostfix(Avatar __instance)
        {
            if (IgnorePatches)
                return;

            OverrideBodyMeasurements(__instance);
        }

        private static bool ValidateAvatar(Avatar avatar, out NetworkPlayer player, out RigManager rm)
        {
            rm = avatar.GetComponentInParent<RigManager>();
            player = null;

            // Make sure this isn't a RealHeptaAvatar avatar! We don't want to scale those values!
            return rm != null && NetworkPlayerManager.TryGetPlayer(rm, out player) && !avatar.TryCast<RealHeptaAvatar>() && player.AvatarSetter.AvatarStats != null;
        }

        private static bool ValidateStats(Avatar __instance, NetworkPlayer player, SerializedAvatarStats stats)
        {
            // Make sure this is the server before validating
            if (!NetworkInfo.IsServer)
            {
                return true;
            }

            // Get permission level of the player
            FusionPermissions.FetchPermissionLevel(player.PlayerId, out var level, out _);

            bool isPolyblank = __instance.name.Contains(FusionAvatar.POLY_BLANK_NAME);

            // Check if this player is using a stat changer
            // We don't check polyblank as it could be a custom avatar
            if (!isPolyblank && !FusionPermissions.HasSufficientPermissions(level, FusionPreferences.LocalServerSettings.StatChangersAllowed.GetValue()))
            {
                float leeway = FusionPreferences.LocalServerSettings.StatChangerLeeway.GetValue();
                leeway = ManagedMathf.Clamp(leeway + 1f, 1f, 11f);

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

            return true;
        }

        private static void OverrideBodyMeasurements(Avatar __instance)
        {
            try
            {
                if (NetworkInfo.HasServer && ValidateAvatar(__instance, out var rep, out var rm))
                {
                    var newStats = rep.AvatarSetter.AvatarStats;

                    // Make sure the stats are valid before applying them
                    if (!ValidateStats(__instance, rep, newStats))
                    {
                        MelonCoroutines.Start(CoKickStatChangerRoutine(__instance, rep));
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

        // We wait a little bit before kicking the user, just to make sure their avatar wasn't incorrectly found guilty mid change
        private static IEnumerator CoKickStatChangerRoutine(Avatar avatar, NetworkPlayer player)
        {
            // Wait two seconds
            float start = TimeUtilities.TimeSinceStartup;
            while (TimeUtilities.TimeSinceStartup - start <= 2f)
                yield return null;

            // Does the RigManager still exist?
            if (!player.HasRig)
                yield break;

            // Does the avatar still apply?
            if (player.RigReferences.RigManager.avatar != avatar)
                yield break;

            // Go ahead and disconnect the user
            ConnectionSender.SendDisconnect(player.PlayerId, "Stat changers are not allowed on this server. Your stats appear to be modified.");

            string username;
            if (player.PlayerId.TryGetDisplayName(out var name))
                username = name;
            else
                username = player.PlayerId.LongId.ToString();

            // Notify the host, they may not know this is a thing
            FusionNotifier.Send(new FusionNotification()
            {
                isMenuItem = false,
                isPopup = true,
                title = "Cheat Detected",
                type = NotificationType.WARNING,
                message = $"{username} was kicked due to modified stats. (This can be disabled.)",
                showTitleOnPopup = true,
            });
        }
    }
}
