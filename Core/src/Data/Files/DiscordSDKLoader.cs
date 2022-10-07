using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Discord;

using MelonLoader;

using LabFusion.Modularity;

namespace LabFusion.Data
{
    public static class DiscordSDKLoader {
        private static IntPtr _libraryPtr;

        public static void OnLoadGameSDK() {
            // Extracts discord game sdk and loads it into the game
            string sdkPath = PersistentData.GetPath($"{Constants.DllName}.dll");
            File.WriteAllBytes(sdkPath, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.GameSDKPath));

            _libraryPtr = DllTools.LoadLibrary(sdkPath);

        }

        public static void OnFreeGameSDK() {
            DllTools.FreeLibrary(_libraryPtr);
        }
    }
}
