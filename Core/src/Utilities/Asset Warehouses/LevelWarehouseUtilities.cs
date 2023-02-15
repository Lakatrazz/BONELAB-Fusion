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
using UnityEngine;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Senders;

namespace LabFusion.Utilities {
    public static class LevelWarehouseUtilities {
        // A period of time before the mod thinks the level has finished loading.
        // Prevents strange issues when the player has not initialized.
        public const float LEVEL_LOAD_WINDOW = 0.5f;

        public const string LOADING_SCREEN_BARCODE = "fa534c5a83ee4ec6bd641fec424c4142.Level.DefaultLoad";

        public const string MOD_SCREEN_BARCODE = "SLZ.BONELAB.CORE.Level.LevelModLevelLoad";

        internal static bool IsLoadingAllowed = false;

        private static object _activeLoadCoroutine;
        private static string _targetLevelBarcode;
        private static string _prevLevelBarcode = "NONE";

        private static bool _isLoading = false;
        private static bool _wasLoading = false;

        private static bool _isDelayedLoading = false;
        private static float _loadingTimer = 0f;

        public static LevelCrate GetCurrentLevel() {
            return SceneStreamer.Session.Level;
        }

        private static bool IsLoading_Internal() {
            return SceneStreamer.Session.Status == StreamStatus.LOADING;
        }

        public static bool IsLoading() => _isLoading;

        public static bool IsDelayedLoading() => _isDelayedLoading;

        public static bool IsLoadDone() => !_isLoading;

        public static bool IsDelayedLoadDone() => !_isDelayedLoading;

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
            while (IsDelayedLoading())
                yield return null;
            
            SendToStreamer();
        }

        internal static void OnUpdateLevelLoading() {
            // If we are in the loading screen we need to make sure to reset this value
            // Otherwise, reloading the scene will never notify the game
            if (IsLoading_Internal()) {
                _prevLevelBarcode = null;
                _isLoading = true;

                // Update loading state
                if (!_wasLoading) {
                    SendLoadingState(true);

                    // Send level load
                    if (NetworkInfo.IsServer)
                        LoadSender.SendLevelLoad(GetCurrentLevel().Barcode);

                    MultiplayerHooking.Internal_OnLoadingBegin();
                }
            }
            else if (_prevLevelBarcode == null) {
                _isLoading = false;

                FusionMod.OnMainSceneInitialized();
                _prevLevelBarcode = GetCurrentLevel().Barcode;

                SendLoadingState(false);
            }

            _wasLoading = _isLoading;

            // Delayed loading
            // For some events, we want to make sure all scripts have initialized
            if (_isLoading) {
                _loadingTimer = 0f;
                _isDelayedLoading = true;
            }
            else if (_loadingTimer <= LEVEL_LOAD_WINDOW) {
                _loadingTimer += Time.deltaTime;
                _isDelayedLoading = true;
            }
            else if (_isDelayedLoading) {
                _isDelayedLoading = false;
                FusionMod.OnMainSceneInitializeDelayed();
            }
        }

        internal static void SendLoadingState(bool isLoading) {
            if (!NetworkInfo.HasServer || PlayerIdManager.LocalId == null)
                return;

            // Set the loading metadata
            PlayerIdManager.LocalId.TrySetMetadata(MetadataHelper.LoadingKey, isLoading.ToString());
        }
    }
}
