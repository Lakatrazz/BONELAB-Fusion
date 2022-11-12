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
        private static string _prevLevelBarcode = null;

        private static bool _hasInitializedScene = false;

        public static LevelCrate GetCurrentLevel() {
            return SceneStreamer.Session.Level;
        }

        private static bool IsLoading_Internal() {
            return SceneStreamer.Session.Status == StreamStatus.LOADING;
        }

        public static bool IsLoading() {
            return IsLoading_Internal() || !_hasInitializedScene;
        }

        private static bool IsLoadDone_Internal() {
            return SceneStreamer.Session.Status == StreamStatus.DONE;
        }

        public static bool IsLoadDone() {
            return IsLoading_Internal() && _hasInitializedScene;
        }

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
            while (IsLoading() || !IsLoadDone())
                yield return null;

            for (var i = 0; i < 60; i++)
                yield return null;

            SendToStreamer();
        }

        internal static void OnUpdateLevelLoading() {
            // If the loading has finished, we can check to update the load method
            if (IsLoadDone_Internal()) {
                var code = GetCurrentLevel().Barcode;

                if (_prevLevelBarcode != code) {
                    FusionMod.OnMainSceneInitialized();
                    _hasInitializedScene = true;
                    _prevLevelBarcode = code;
                }
            }
            // If we are in the loading screen we need to make sure to reset this value
            // Otherwise, reloading the scene will never notify the game
            else if (IsLoading_Internal()) {
                _prevLevelBarcode = null;
                _hasInitializedScene = false;
            }
        }
    }
}
