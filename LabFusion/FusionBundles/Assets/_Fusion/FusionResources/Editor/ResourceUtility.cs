using System.IO;

using UnityEditor;
using UnityEngine;

namespace MarrowFusion.Editor
{
    public static class ResourceUtility
    {
        public static readonly string BuiltPalletsName = "BuiltPallets";

        public static readonly string FusionResourcesName = "_Fusion/FusionResources";

        public static readonly string ContentPalletTitle = "Lakatrazz.FusionContent";

        [MenuItem("Fusion/Resources/Copy Resources To Content Pallets")]
        public static void CopyResourcesToContentPallets()
        {
            var assetsPath = Application.dataPath;

            var parentPath = Path.GetDirectoryName(assetsPath);
            var builtPalletsPath = Path.Combine(parentPath, BuiltPalletsName);

            var fusionResourcesPath = Path.Combine(assetsPath, FusionResourcesName);

            CopyResourcesToContentPallet(builtPalletsPath, fusionResourcesPath, BuildTarget.StandaloneWindows64);
            CopyResourcesToContentPallet(builtPalletsPath, fusionResourcesPath, BuildTarget.Android);

            Debug.Log("Successfully copied resources to the built content pallets!");
        }

        private static void CopyResourcesToContentPallet(string builtPalletsPath, string fusionResourcesPath, BuildTarget target)
        {
            var contentPalletPath = Path.Combine(builtPalletsPath, target.ToString(), ContentPalletTitle);

            var targetResourcesPath = Path.Combine(fusionResourcesPath, target.ToString());

            if (!Directory.Exists(targetResourcesPath))
            {
                return;
            }

            var outputResourcesPath = Path.Combine(contentPalletPath, FusionResourcesName);

            if (Directory.Exists(outputResourcesPath))
            {
                Directory.Delete(outputResourcesPath, true);
            }

            CopyDirectoryWithoutMeta(targetResourcesPath, outputResourcesPath);
        }

        private static void CopyDirectoryWithoutMeta(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                if (Path.GetExtension(file) == ".meta")
                {
                    continue;
                }

                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetDir, fileName);
                File.Copy(file, destFile, true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                string destSubDir = Path.Combine(targetDir, dirName);
                CopyDirectoryWithoutMeta(subDir, destSubDir);
            }
        }
    }
}