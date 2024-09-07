using LabFusion.Network;
using LabFusion.Patching;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Marrow;
using LabFusion.RPC;
using LabFusion.Player;
using LabFusion.Downloading;
using LabFusion.Preferences.Client;
using LabFusion.Downloading.ModIO;
using LabFusion.Data;

using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Scene;

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

        _hasStartedDownloadingTarget = false;
    }

    private static void Internal_SetServerScene(string barcode, string loadBarcode)
    {
        // This is a brand new scene, so reset the download check
        _hasStartedDownloadingTarget = false;

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
                {
                    LoadSender.SendLevelLoad(Barcode, LoadBarcode);
                }

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
        {
            return;
        }

        // If we have entered the loading screen after beginning to load the target, set the value to true
        if (_hasStartedLoadingTarget && IsLoading())
        {
            _hasEnteredTargetLoadingScreen = true;
        }

        // If we aren't loading and we have a target scene, change to it
        if (IsDelayedLoadDone() && !_hasStartedDownloadingTarget && !_hasStartedLoadingTarget && !string.IsNullOrEmpty(_targetServerScene))
        {
            bool hasLevel = CrateFilterer.HasCrate<LevelCrate>(new(_targetServerScene));

            if (hasLevel)
            {
                LoadTargetScene();
            }
            else
            {
                bool shouldDownload = ClientSettings.Downloading.DownloadLevels.Value;

                // Check if we should download the mod (it's not blacklisted, mod downloading disabled, etc.)
                if (shouldDownload)
                {
                    BeginDownloadingScene();
                }
                // Can't download the level, leave the server
                else
                {
                    OnDownloadFailed();
                }
            }
        }
    }

    private static void BeginDownloadingScene()
    {
        // Cancel all currently queued downloads.
        // They aren't as important as the level, and anything waiting for a download likely won't exist anymore in the new scene.
        ModIODownloader.CancelQueue();

        // Get the maximum amount of bytes that we download before cancelling, to make sure the level isn't too big
        long maxBytes = DataConversions.ConvertMegabytesToBytes(ClientSettings.Downloading.MaxLevelSize.Value);

        // Request the mod id from the host
        NetworkModRequester.RequestAndInstallMod(new NetworkModRequester.ModInstallInfo()
        {
            target = PlayerIdManager.HostSmallId,
            barcode = _targetServerScene,
            finishDownloadCallback = OnDownloadFinished,
            maxBytes = maxBytes,
        });

        _hasStartedDownloadingTarget = true;
    }

    private static void OnDownloadFinished(DownloadCallbackInfo info) 
    {
        if (info.result == ModResult.FAILED)
        {
            OnDownloadFailed();
            return;
        }

        // We can now load the level
        LoadTargetScene();

        _hasStartedDownloadingTarget = false;
    }

    private static void OnDownloadFailed()
    {
        NetworkHelper.Disconnect("The server's level failed to install!");

        _hasStartedDownloadingTarget = false;
    }

    public static void LoadTargetScene()
    {
        SceneLoadPatch.IgnorePatches = true;

        SceneStreamer.Load(new Barcode(_targetServerScene), new Barcode(_targetServerLoadScene));

        SceneLoadPatch.IgnorePatches = false;

        _hasStartedLoadingTarget = true;
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