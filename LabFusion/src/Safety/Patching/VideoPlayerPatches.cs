using HarmonyLib;

using UnityEngine.Video;

namespace LabFusion.Safety.Patching;

[HarmonyPatch(typeof(VideoPlayer))]
public static class VideoPlayerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(VideoPlayer.url))]
    [HarmonyPatch(MethodType.Setter)]
    public static void SetUrlPrefix(VideoPlayer __instance, ref string value)
    {
        // TODO: replace value with null if url isn't whitelisted
    }
}
