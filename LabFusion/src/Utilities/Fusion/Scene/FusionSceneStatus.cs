using LabFusion.Network;
using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Utilities
{
    public static partial class FusionSceneManager
    {
        // Loading logic
        private static bool _isLoading = true;
        private static bool _wasLoading = true;
        private static string _prevLevelBarcode = "NONE";

        // Delayed load logic
        public const float LEVEL_LOAD_WINDOW = 0.5f;
        private static bool _isDelayedLoading = true;
        private static float _loadingTimer = 0f;

        // Target scene logic
        private static string _targetServerScene = string.Empty;
        private static string _targetServerLoadScene = string.Empty;
        private static bool _hasStartedLoadingTarget = false;
        private static bool _hasEnteredTargetLoadingScreen = false;

        public static LevelCrate Level => SceneStreamer.Session.Level;
        public static string Barcode => Level != null ? Level.Barcode : "";
        public static string Title => Level != null ? Level.Title : "";

        public static LevelCrate LoadLevel => SceneStreamer.Session.LoadLevel;
        public static string LoadBarcode => LoadLevel != null ? LoadLevel.Barcode : "";

        public static bool HasLevel(string barcode)
        {
            if (AssetWarehouse.Instance.GetCrate<LevelCrate>(barcode) != null)
                return true;
            else
                return false;
        }

        public static bool IsLoading() => _isLoading;

        public static bool IsDelayedLoading() => _isDelayedLoading;

        public static bool IsLoadDone() => !_isLoading;

        public static bool IsDelayedLoadDone() => !_isDelayedLoading;

        public static bool HasTargetLoaded()
        {
            // If we are the host or have no server, just do the normal load check
            if (!NetworkInfo.HasServer || NetworkInfo.IsServer)
                return IsLoadDone();

            return _hasStartedLoadingTarget && _hasEnteredTargetLoadingScreen && IsLoadDone();
        }

        private static bool IsLoading_Internal()
        {
            return SceneStreamer.Session.Status == StreamStatus.LOADING;
        }
    }
}
