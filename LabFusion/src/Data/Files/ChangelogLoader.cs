using System.Text;

namespace LabFusion.Data
{
    public static class ChangelogLoader
    {
        public static void ReadFile()
        {
#if DEBUG
            FusionMod.Changelog = "- Debug build. Changelog will show in the release build.";
#else
            FusionMod.Changelog = ReadFromPath(ResourcePaths.RawChangelogPath, PersistentData.GetPath($"raw_changelog.txt"));
#endif

            FusionMod.Credits = new string[] {
                ReadFromPath($"{ResourcePaths.RawCreditsPath}1.txt", PersistentData.GetPath($"raw_credits_01.txt")),
                ReadFromPath($"{ResourcePaths.RawCreditsPath}2.txt", PersistentData.GetPath($"raw_credits_02.txt")),
                ReadFromPath($"{ResourcePaths.RawCreditsPath}3.txt", PersistentData.GetPath($"raw_credits_03.txt")),
            };
        }

        private static string ReadFromPath(string resourcePath, string filePath)
        {
            File.WriteAllBytes(filePath, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, resourcePath));

            var lines = File.ReadAllLines(filePath);

            StringBuilder builder = new();

            foreach (var line in lines)
            {
                builder.Append(line);
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
