using LabFusion.Network;
using SLZ.Marrow.SceneStreaming;
using SLZ.Marrow.Warehouse;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities
{
    public static partial class FusionSceneManager
    {
        // Loading logic
        private static bool _isLoading = false;
        private static bool _wasLoading = false;
        private static string _prevLevelBarcode = "NONE";

        // Delayed load logic
        public const float LEVEL_LOAD_WINDOW = 0.5f;
        private static bool _isDelayedLoading = false;
        private static float _loadingTimer = 0f;

        // Target scene logic
        private static string _targetServerScene = null;
        private static bool _hasStartedLoadingTarget = false;
        private static bool _hasEnteredTargetLoadingScreen = false;

        public static LevelCrate Level => SceneStreamer.Session.Level;
        public static string Barcode => Level.Barcode;
        public static string Title => Level.Title;

        public static bool IsLoading() => _isLoading;

        public static bool IsDelayedLoading() => _isDelayedLoading;

        public static bool IsLoadDone() => !_isLoading;

        public static bool IsDelayedLoadDone() => !_isDelayedLoading;

        public static bool HasTargetLoaded() {
            if (NetworkInfo.IsServer)
                return IsLoadDone();

            return _hasStartedLoadingTarget && _hasEnteredTargetLoadingScreen && IsLoadDone();
        }

        private static bool IsLoading_Internal() {
            return SceneStreamer.Session.Status == StreamStatus.LOADING;
        }
    }
}
