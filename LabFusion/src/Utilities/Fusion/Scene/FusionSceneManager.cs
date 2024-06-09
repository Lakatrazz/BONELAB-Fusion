using LabFusion.Network;
using LabFusion.Patching;
using LabFusion.Senders;

using Il2CppSLZ.Marrow.SceneStreaming;

namespace LabFusion.Utilities
{
    public static partial class FusionSceneManager
    {
        internal static void Internal_OnInitializeMelon()
        {
            // Hook into events
            MultiplayerHooking.OnStartServer += Internal_OnCleanup;
            MultiplayerHooking.OnDisconnect += Internal_OnCleanup;
        }

        private static void Internal_OnCleanup()
        {
            // Reset target scenes
            _targetServerScene = string.Empty;
            _targetServerLoadScene = string.Empty;
            _hasStartedLoadingTarget = false;
            _hasEnteredTargetLoadingScreen = false;
        }

        private static void Internal_SetServerScene(string barcode, string loadBarcode)
        {
            // Here we set the target server scene
            // This is the scene barcode sent by the server to the client, which we want to load
            _targetServerScene = barcode;
            _targetServerLoadScene = loadBarcode;
            _hasStartedLoadingTarget = false;
            _hasEnteredTargetLoadingScreen = false;
        }

        private static void Internal_UpdateLoadStatus()
        {
            if (IsLoading_Internal())
            {
                _prevLevelBarcode = null;
                _isLoading = true;

                // Update loading state
                if (!_wasLoading)
                {
                    LoadSender.SendLoadingState(true);

                    // Send level load
                    if (NetworkInfo.IsServer)
                        LoadSender.SendLevelLoad(Barcode, LoadBarcode);

                    MultiplayerHooking.Internal_OnLoadingBegin();
                }
            }
            else if (_prevLevelBarcode == null)
            {
                _isLoading = false;

                FusionMod.OnMainSceneInitialized();
                _prevLevelBarcode = Barcode;

                LoadSender.SendLoadingState(false);

                // Invoke the level load hook
                _onLevelLoad?.Invoke();
                _onLevelLoad = null;

                // If the target was loaded, invoke that hook
                if (HasTargetLoaded())
                {
                    _onTargetLevelLoad?.Invoke();
                    _onTargetLevelLoad = null;
                }
            }

            _wasLoading = _isLoading;

        }

        private static void Internal_UpdateDelayedLoadStatus()
        {
            if (_isLoading)
            {
                _loadingTimer = 0f;
                _isDelayedLoading = true;
            }
            else if (_loadingTimer <= LEVEL_LOAD_WINDOW)
            {
                _loadingTimer += TimeUtilities.DeltaTime;
                _isDelayedLoading = true;
            }
            else if (_isDelayedLoading)
            {
                _isDelayedLoading = false;
                FusionMod.OnMainSceneInitializeDelayed();

                // Invoke the level load hook
                _onDelayedLevelLoad?.Invoke();
                _onDelayedLevelLoad = null;
            }
        }

        private static void Internal_UpdateTargetScene()
        {
            // Make sure we are a client and have loaded
            if (!NetworkInfo.IsClient)
                return;

            // If we have entered the loading screen after beginning to load the target, set the value to true
            if (_hasStartedLoadingTarget && IsLoading())
            {
                _hasEnteredTargetLoadingScreen = true;
            }

            // If we aren't loading and we have a target scene, change to it
            if (IsDelayedLoadDone() && !_hasStartedLoadingTarget && !string.IsNullOrEmpty(_targetServerScene))
            {
                SceneLoadPatch.IgnorePatches = true;
                SceneStreamer.Load(_targetServerScene, _targetServerLoadScene);
                SceneLoadPatch.IgnorePatches = false;

                _hasStartedLoadingTarget = true;
            }
        }

        internal static void Internal_UpdateScene()
        {
            // Loading status update
            Internal_UpdateLoadStatus();

            // Delayed loading update
            Internal_UpdateDelayedLoadStatus();

            // Target level loading update
            Internal_UpdateTargetScene();
        }

        public static void SetTargetScene(string barcode, string loadBarcode)
        {
            Internal_SetServerScene(barcode, loadBarcode);
        }
    }
}
