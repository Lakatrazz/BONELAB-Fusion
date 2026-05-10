namespace LabFusion.Data;

public static class ChangelogLoader
{
    public static void ReadFile()
    {
#if DEBUG
        FusionMod.Changelog = "- Debug build. Changelog will show in the release build.";
#else
        FusionMod.Changelog = ReadResource(ResourcePaths.RawChangelogPath);
#endif

        FusionMod.Credits = new string[] {
            ReadResource($"{ResourcePaths.RawCreditsPath}1.txt"),
            ReadResource($"{ResourcePaths.RawCreditsPath}2.txt"),
            ReadResource($"{ResourcePaths.RawCreditsPath}3.txt"),
        };
    }

    private static string ReadResource(string resourcePath)
    {
        var text = EmbeddedResource.LoadTextFromAssembly(FusionMod.FusionAssembly, resourcePath);

        return text;
    }
}
