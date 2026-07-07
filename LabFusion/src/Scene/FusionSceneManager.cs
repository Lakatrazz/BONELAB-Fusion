using LabFusion.Network;
using LabFusion.Senders;
using LabFusion.Utilities;
using LabFusion.Marrow;
using LabFusion.Player;
using LabFusion.Preferences.Client;
using LabFusion.Marrow.Patching;
using LabFusion.Extensions;
using LabFusion.Marrow.Serialization;

using Il2CppSLZ.Marrow.SceneStreaming;
using Il2CppSLZ.Marrow.Warehouse;

namespace LabFusion.Scene;

public static partial class FusionSceneManager
{
    internal static void Internal_OnInitializeMelon()
    {
        // Hook into events
        MultiplayerHooking.OnStartedServer += Internal_OnCleanup;
        MultiplayerHooking.OnDisconnected += Internal_OnCleanup;

        // Prepare level downloading
        LevelDownloaderManager.OnInitializeMelon();
    }

    private static void Internal_OnCleanup()
    {
        // Reset target scenes
        _targetServerScene = SerializedCrateReference.None;
        _targetServerLoadScene = string.Empty;
        _hasStartedLoadingTarget = false;
        _hasEnteredTargetLoadingScreen = false;

        _hasStartedDownloadingTarget = false;
    }

    private static void Internal_SetServerScene(SerializedCrateReference levelReference, string loadBarcode)
    {
        // This is a brand new scene, so reset the download check
        _hasStartedDownloadingTarget = false;

        // Here we set the target server scene
        // This is the scene barcode sent by the server to the client, which we want to load
        _targetServerScene = levelReference;
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
                LocalPlayer.Metadata.LevelBarcode.SetValue(Barcode);

                // Send level load
                if (NetworkInfo.IsHost)
                {
                    LoadSender.SendLevelLoad(Barcode, LoadBarcode);
                }

                MultiplayerHooking.InvokeOnLoadingBegin();
            }
        }
        else if (_prevLevelBarcode == null)
        {
            _isLoading = false;

            FusionMod.OnMainSceneInitialized();
            _prevLevelBarcode = Barcode;

            LoadSender.SendLoadingState(!HasTargetLoaded());
            LocalPlayer.Metadata.LevelBarcode.SetValue(Barcode);

            // Invoke the level load hook
            _onLevelLoad?.InvokeSafe("executing OnLevelLoad hook");
            _onLevelLoad = null;

            // If the target was loaded, invoke that hook
            if (HasTargetLoaded())
            {
                _onTargetLevelLoad?.InvokeSafe("executing OnTargetLevelLoad hook");
                _onTargetLevelLoad = null;

                MultiplayerHooking.InvokeTargetLevelLoaded();
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
            _loadingTimer += TimeReferences.DeltaTime;
            _isDelayedLoading = true;
        }
        else if (_isDelayedLoading)
        {
            _isDelayedLoading = false;
            FusionMod.OnMainSceneInitializeDelayed();

            // Invoke the level load hook
            _onDelayedLevelLoad?.InvokeSafe("executing OnDelayedLevelLoad hook");
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
        if (IsDelayedLoadDone() && !_hasStartedDownloadingTarget && !_hasStartedLoadingTarget && _targetServerScene.IsValid)
        {
            bool hasLevel = _targetServerScene.HasCrate<LevelCrate>();

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
                    LevelDownloaderManager.DownloadLevel(new LevelDownloaderManager.LevelDownloadInfo()
                    {
                        LevelReference = _targetServerScene,
                        LevelHost = PlayerIDManager.HostSmallID,
                        OnDownloadSucceeded = OnDownloadSucceeded,
                        OnDownloadFailed = OnDownloadFailed,
                        OnDownloadCanceled = OnDownloadCanceled,
                    });

                    _hasStartedDownloadingTarget = true;
                }
                // Can't download the level, leave the server
                else
                {
                    OnDownloadFailed();
                }
            }
        }
    }

    private static void OnDownloadSucceeded() 
    {
        // We can now load the level
        // Hook the level load incase we're in the loading screen
        HookOnDelayedLevelLoad(LoadTargetScene);

        _hasStartedDownloadingTarget = false;
    }

    private static void OnDownloadFailed()
    {
        NetworkHelper.Disconnect("The server's level failed to install!");

        _hasStartedDownloadingTarget = false;
    }

    private static void OnDownloadCanceled()
    {
        _hasStartedDownloadingTarget = false;
    }

    public static void LoadTargetScene()
    {
        SceneStreamerPatches.IgnorePatches = true;

        SceneStreamer.Load(new Barcode(_targetServerScene.Barcode), new Barcode(_targetServerLoadScene));

        SceneStreamerPatches.IgnorePatches = false;

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

    public static void SetTargetScene(SerializedCrateReference levelReference, string loadBarcode)
    {
        Internal_SetServerScene(levelReference, loadBarcode);
    }
}