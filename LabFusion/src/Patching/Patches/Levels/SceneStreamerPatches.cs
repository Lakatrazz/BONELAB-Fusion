using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;

using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(SceneStreamer))]
    public class SceneLoadPatch
    {
        public static bool IgnorePatches = false;

        [HarmonyPatch(nameof(SceneStreamer.Reload))]
        [HarmonyPrefix]
        public static bool Reload()
        {
            // Check if we need to exit early
            if (!IgnorePatches && NetworkInfo.HasServer && !NetworkInfo.IsServer)
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(SceneStreamer.Load), typeof(string), typeof(string))]
        [HarmonyPrefix]
        public static bool StringLoad(string levelBarcode, string loadLevelBarcode = "")
        {
            // Check if we need to exit early
            if (!IgnorePatches && NetworkInfo.HasServer && !NetworkInfo.IsServer)
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(SceneStreamer.Load), typeof(LevelCrateReference), typeof(LevelCrateReference))]
        [HarmonyPrefix]
        public static bool CrateLoad(LevelCrateReference level, LevelCrateReference loadLevel)
        {
            try
            {
                // Check if we need to exit early
                if (!IgnorePatches && NetworkInfo.HasServer && !NetworkInfo.IsServer)
                {
                    return false;
                }
            }

            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch SceneStreamer.Load", e);
#endif
            }

            return true;
        }
    }
}
