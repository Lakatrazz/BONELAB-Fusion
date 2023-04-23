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
    public static class SteamAPILoader {
        public static bool HasSteamAPI { get; private set; } = false;

        private static IntPtr _libraryPtr;

        public static void OnLoadSteamAPI() {
            // If it's already loaded, don't load it again
            if (HasSteamAPI)
                return;

            // Don't extract this for android
            if (HelperMethods.IsAndroid()) {
                HasSteamAPI = false;
                return;
            }

            // Extracts steam api 64 and loads it into the game
            string sdkPath = PersistentData.GetPath($"steam_api64.dll");
            File.WriteAllBytes(sdkPath, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, ResourcePaths.SteamAPIPath));

            _libraryPtr = DllTools.LoadLibrary(sdkPath);

            if (_libraryPtr != IntPtr.Zero) {
                FusionLogger.Log("Successfully loaded steam_api64.dll into the application!");
                HasSteamAPI = true;
            }
            else {
                uint errorCode = DllTools.GetLastError();
                FusionLogger.Error($"Failed to load steam_api64.dll into the application.\nError Code: {errorCode}");
            }
        }

        public static void OnFreeSteamAPI() {
            DllTools.FreeLibrary(_libraryPtr);

            HasSteamAPI = false;
        }
    }
}
