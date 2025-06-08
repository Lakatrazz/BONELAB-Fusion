using HarmonyLib;

using LabFusion.Scene;

using UnityEngine.Video;

namespace LabFusion.Safety.Patching;

[HarmonyPatch(typeof(VideoPlayer))]
public static class VideoPlayerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(VideoPlayer.Prepare))]
    public static bool PreparePrefix(VideoPlayer __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (__instance.source == VideoSource.Url && !URLWhitelistManager.IsURLWhitelisted(__instance.url))
        {
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(VideoPlayer.url))]
    [HarmonyPatch(MethodType.Setter)]
    public static void SetUrlPrefix(ref string value)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        if (!URLWhitelistManager.IsURLWhitelisted(value))
        {
            value = string.Empty;
        }
    }
}
