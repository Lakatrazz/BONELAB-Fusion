using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using LabFusion.Modularity;

namespace LabFusion.Data
{
    public static class SteamAPILoader {
        private static IntPtr _libraryPtr;

        public static void OnLoadSteamAPI() {
            // Extracts steam api 64 and loads it into the game
            string sdkPath = PersistentData.GetPath($"steam_api64.dll");
            File.WriteAllBytes(sdkPath, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.SteamAPIPath));

            _libraryPtr = DllTools.LoadLibrary(sdkPath);

        }

        public static void OnFreeSteamAPI() {
            DllTools.FreeLibrary(_libraryPtr);
        }
    }
}
