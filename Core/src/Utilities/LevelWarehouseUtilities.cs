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

        public static LevelCrate GetCurrentLevel() {
            return SceneStreamer.Session.Level;
        }

        public static bool IsLoading() {
            return SceneStreamer.Session.Status == StreamStatus.LOADING;
        }

        public static bool IsLoadDone() {
            return SceneStreamer.Session.Status == StreamStatus.DONE;
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
    }
}
