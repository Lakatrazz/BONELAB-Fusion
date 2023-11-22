using LabFusion.Network;
using SLZ.Marrow.SceneStreaming;
using SLZ.Marrow.Warehouse;

namespace LabFusion.Utilities
{
    public static partial class FusionSceneManager
    {
        // Loading logic
        private static bool _isLoading;
        private static bool _wasLoading;
        private static string _prevLevelBarcode = "NONE";

        // Delayed load logic
        public const float LEVEL_LOAD_WINDOW = 0.5f;
        private static bool _isDelayedLoading;
        private static float _loadingTimer;

        // Target scene logic
        private static string _targetServerScene;
        private static bool _hasStartedLoadingTarget;
        private static bool _hasEnteredTargetLoadingScreen;

        public static LevelCrate Level => SceneStreamer.Session.Level;
        public static string Barcode => Level != null ? Level.Barcode : "";
        public static string Title => Level != null ? Level.Title : "";

        public static bool HasLevel(string barcode)
        {
            if (AssetWarehouse.Instance.GetCrate<LevelCrate>(barcode) != null)
                return true;
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
