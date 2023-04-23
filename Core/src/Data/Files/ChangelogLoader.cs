using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using LabFusion.Utilities;

using BoneLib;

namespace LabFusion.Data
{
    public static class ChangelogLoader
    {
        public static void ReadFromFile()
        {
#if DEBUG
            FusionMod.Changelog = "- Debug build. Changelog will show in the release build.";
#else

            // Extracts the changelog into the appdata folder
            string sdkPath = PersistentData.GetPath($"raw_changelog.txt");
            File.WriteAllBytes(sdkPath, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.RawChangelogPath));

            // Set the changelog text of the mod
            var lines = File.ReadAllLines(sdkPath);

            StringBuilder builder = new StringBuilder();

            foreach (var line in lines) {
                builder.Append(line);
                builder.AppendLine();
            }

            FusionMod.Changelog = builder.ToString();
#endif
        }
    }
}
