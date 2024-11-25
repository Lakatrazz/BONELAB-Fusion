using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Entities;

using Avatar = Il2CppSLZ.VRMK.Avatar;

using Il2CppSLZ.Marrow;

namespace LabFusion.Patching;

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

    private static void OverrideBodyMeasurements(Avatar __instance)
    {
        try
        {
            if (NetworkInfo.HasServer && ValidateAvatar(__instance, out var rep, out var rm))
            {
                var newStats = rep.AvatarSetter.AvatarStats;

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