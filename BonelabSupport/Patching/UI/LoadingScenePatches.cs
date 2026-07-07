using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Scene;

namespace MarrowFusion.Bonelab.Patching;

[HarmonyPatch(typeof(LoadingScene))]
public static class LoadingScenePatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(LoadingScene.Start))]
    public static void StartPostfix(LoadingScene __instance)
    {
        if (LevelDownloaderManager.IsDownloadingLevel)
        {
            ApplyLevelDownloadPreview(__instance);
        }
    }

    private static void ApplyLevelDownloadPreview(LoadingScene __instance)
    {
        string newTitle = $"{LevelDownloaderManager.TargetLevel.Title} [Downloading]";

        foreach (var text in __instance.title_sceneName)
        {
            text.text = newTitle;
        }
    }
}
