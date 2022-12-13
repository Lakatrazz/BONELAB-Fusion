using LabFusion.Patching;

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SLZ.Marrow.SceneStreaming;
using SLZ.Marrow.Warehouse;
using MelonLoader;

namespace LabFusion.Utilities {
    public static class LevelWarehouseUtilities {
        public const string LOADING_SCREEN_BARCODE = "fa534c5a83ee4ec6bd641fec424c4142.Level.DefaultLoad";

        public const string MOD_SCREEN_BARCODE = "SLZ.BONELAB.CORE.Level.LevelModLevelLoad";

        internal static bool IsLoadingAllowed = false;

        private static object _activeLoadCoroutine;
        private static string _targetLevelBarcode;
        private static string _prevLevelBarcode = "NONE";

        private static bool _isLoading = false;

        public static LevelCrate GetCurrentLevel() {
            return SceneStreamer.Session.Level;
        }

        private static bool IsLoading_Internal() {
            return SceneStreamer.Session.Status == StreamStatus.LOADING;
        }

        public static bool IsLoading() => _isLoading;

        public static bool IsLoadDone() => !_isLoading;

        public static void LoadClientLevel(string levelBarcode) {
            _targetLevelBarcode = levelBarcode;

            if (_activeLoadCoroutine == null)
                _activeLoadCoroutine = MelonCoroutines.Start(LoadLevelDelayed());
        }

        internal static void SendToStreamer() {
            _activeLoadCoroutine = null;

            IsLoadingAllowed = true;
            SceneStreamer.Load(_targetLevelBarcode, LOADING_SCREEN_BARCODE);
            IsLoadingAllowed = false;
        }

        internal static IEnumerator LoadLevelDelayed() {
            if (IsLoading()) {
                while (IsLoading())
                    yield return null;

                for (var i = 0; i < 60; i++)
                    yield return null;
            }
            
            SendToStreamer();
        }

        internal static void OnUpdateLevelLoading() {
            // If we are in the loading screen we need to make sure to reset this value
            // Otherwise, reloading the scene will never notify the game
            if (IsLoading_Internal()) {
                _prevLevelBarcode = null;
                _isLoading = true;
            }
            else if (_prevLevelBarcode == null) {
                FusionMod.OnMainSceneInitialized();
                _prevLevelBarcode = GetCurrentLevel().Barcode;
                _isLoading = false;
            }
        }
    }
}
