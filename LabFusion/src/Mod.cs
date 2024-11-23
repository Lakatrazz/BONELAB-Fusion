using System.Reflection;

using UnityEngine;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Scene;
using LabFusion.Grabbables;
using LabFusion.Preferences;
using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Points;
using LabFusion.SDK.Achievements;
using LabFusion.Voice;
using LabFusion.SDK.Lobbies;
using LabFusion.SDK.Cosmetics;
using LabFusion.Entities;
using LabFusion.Downloading.ModIO;
using LabFusion.Downloading;
using LabFusion.Marrow;
using LabFusion.Menu;
using LabFusion.SDK.Modules;
using LabFusion.Bonelab;
using LabFusion.Representation;
using LabFusion.Player;

#if DEBUG
using LabFusion.Debugging;
#endif

using MelonLoader;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;
using LabFusion.RPC;

namespace LabFusion;

public struct FusionVersion
{
#if DEBUG
    public const byte VersionMajor = 0;
    public const byte VersionMinor = 0;
    public const short VersionPatch = 0;

    public const string VersionString = "0.0.0";
#else
    public const byte VersionMajor = 1;
    public const byte VersionMinor = 9;
    public const short VersionPatch = 0;

    public const string VersionString = "1.9.0";
#endif
}

public class FusionMod : MelonMod
{
    public const string ModName = "LabFusion";
    public const string ModAuthor = "Lakatrazz";

    public const string GameDeveloper = "Stress Level Zero";
    public const string GameName = "BONELAB";

    public static readonly Version Version = new(FusionVersion.VersionMajor, FusionVersion.VersionMinor, FusionVersion.VersionPatch);

    public static string Changelog { get; internal set; } = null;

    public static string[] Credits { get; internal set; } = null;

    public static FusionMod Instance { get; private set; }
    public static Assembly FusionAssembly { get; private set; }

    private static bool _hasAutoUpdater = false;

    private static int _nextSyncableSendRate = 1;

    public override void OnEarlyInitializeMelon()
    {
        Instance = this;
        FusionAssembly = MelonAssembly.Assembly;

        // Delete temporary downloads from the last session
        ModDownloadManager.DeleteTemporaryDirectories();

        // Prepare the data path for writing files
        PersistentData.OnPathInitialize();

        // Load APIs
        SteamAPILoader.OnLoadSteamAPI();

        // Initialize data and hooks
        ByteRetriever.PopulateInitial();
        PDController.OnInitializeMelon();
        PointItemManager.HookEvents();
    }

    public override void OnInitializeMelon()
    {
        // Pull files
        FusionFileLoader.OnInitializeMelon();

        // Load assetbundles
        FusionBundleLoader.OnBundleLoad();

        // Initialize player
        FusionPlayer.OnInitializeMelon();
        LocalPlayer.OnInitializeMelon();
        LocalVision.OnInitializeMelon();

        // Register base modules
        InitializeBaseModules();

        // Register our base handlers
        LevelDataHandler.OnInitializeMelon();
        FusionMessageHandler.RegisterHandlersFromAssembly(FusionAssembly);
        GrabGroupHandler.RegisterHandlersFromAssembly(FusionAssembly);
        NetworkLayer.RegisterLayersFromAssembly(FusionAssembly);
        GamemodeRegistration.LoadGamemodes(FusionAssembly);
        PointItemManager.LoadItems(FusionAssembly);
        AchievementManager.LoadAchievements(FusionAssembly);

        EntityComponentManager.RegisterComponentsFromAssembly(FusionAssembly);

        LobbyFilterManager.LoadBuiltInFilters();

        NetworkEntityManager.OnInitializeManager();
        NetworkPlayerManager.OnInitializeManager();

        FusionPopupManager.OnInitializeMelon();

        GamemodeManager.OnInitializeMelon();
        GamemodeConditionsChecker.OnInitializeMelon();

        // Hook into asset warehouse
        var onReady = () =>
        {
            CosmeticLoader.OnAssetWarehouseReady();
            ScannableEvents.OnAssetWarehouseReady();
        };
        AssetWarehouse.OnReady(onReady);

        // Create prefs
        FusionPreferences.OnInitializePreferences();

        FusionPermissions.OnInitializeMelon();

        LobbyInfoManager.OnInitialize();

        MenuCreator.OnInitializeMelon();

        // Initialize level loading
        FusionSceneManager.Internal_OnInitializeMelon();

        // Finally, initialize the network layer
        OnInitializeNetworking();

#if DEBUG
        FusionUnityLogger.OnInitializeMelon();
#endif
    }

    private static void InitializeBaseModules()
    {
        ModuleManager.RegisterModule<MarrowModule>();
        ModuleManager.RegisterModule<BonelabModule>();
    }

    public override void OnLateInitializeMelon()
    {
        InternalLayerHelpers.OnLateInitializeLayer();
        PersistentAssetCreator.OnLateInitializeMelon();
        PlayerAdditionsHelper.OnInitializeMelon();

#if RELEASE
        // Check if the auto updater is installed
        _hasAutoUpdater = MelonPlugin.RegisteredMelons.Any((p) => p.Info.Name.Contains("LabFusion Updater"));

        if (!_hasAutoUpdater)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                SaveToMenu = false,
                ShowPopup = true,
                Message = "You do not have the Fusion AutoUpdater installed in your plugins folder!" +
                "\nIt is recommended to install it in order to stay up to date.",
                Type = NotificationType.WARNING,
            });
        }
#endif
    }

    protected static void OnInitializeNetworking()
    {
        // If a layer is already set, don't initialize
        if (NetworkInfo.CurrentNetworkLayer != null)
        {
            FusionLogger.Warn("Cannot initialize new network layer because a previous one is active!");
            return;
        }

        // Validate the layer
        NetworkLayerDeterminer.LoadLayer();

        if (NetworkLayerDeterminer.LoadedLayer == null)
        {
            FusionLogger.Error("The target network layer is null!");
            return;
        }

        // Finally, set the layer
        InternalLayerHelpers.SetLayer(NetworkLayerDeterminer.LoadedLayer);
    }

    public override void OnDeinitializeMelon()
    {
        // Cleanup networking
        InternalLayerHelpers.OnCleanupLayer();

        // Backup files
        FusionFileLoader.OnDeinitializeMelon();

        // Unhook assembly loads
        PointItemManager.UnhookEvents();

        // Unload assetbundles
        FusionBundleLoader.OnBundleUnloaded();

        // Undo game changes
        PlayerAdditionsHelper.OnDeinitializeMelon();

        // Free APIs
        SteamAPILoader.OnFreeSteamAPI();
    }

    public override void OnPreferencesLoaded()
    {
        FusionPreferences.OnPreferencesLoaded();
    }

    public static void OnMainSceneInitialized()
    {
        string sceneName = FusionSceneManager.Level.Title;

#if DEBUG
        FusionLogger.Log($"Main scene {sceneName} was initialized.");
#endif

        // Cache info
        NetworkEntityManager.OnCleanupIds();

        RigData.OnCacheRigInfo();
        PersistentAssetCreator.OnMainSceneInitialized();
        ConstrainerUtilities.OnMainSceneInitialized();

        // Update hooks
        MultiplayerHooking.Internal_OnMainSceneInitialized();

        FusionPlayer.OnMainSceneInitialized();
    }

    public static void OnMainSceneInitializeDelayed()
    {
        // Make sure the rig exists
        if (!RigData.HasPlayer)
        {
            return;
        }

        // Force enable radial menu
        RigData.Refs.RigManager.ControllerRig.TryCast<OpenControllerRig>().quickmenuEnabled = true;
        PlayerRefs.Instance.PlayerBodyVitals.quickmenuEnabled = true;

        // Create the Fusion Menu
        MenuCreator.CreateMenu();
    }

    public override void OnUpdate()
    {
        // Reset byte counts
        NetworkInfo.BytesDown = 0;
        NetworkInfo.BytesUp = 0;

        // Process mod downloads
        ModIODownloader.UpdateQueue();

        // Update Time before running any functions
        TimeUtilities.OnEarlyUpdate();

        // Update the level loading checks
        FusionSceneManager.Internal_UpdateScene();

        // Update popups
        FusionPopupManager.OnUpdate();

        // Update network players
        float deltaTime = TimeUtilities.DeltaTime;

        NetworkPlayerManager.OnUpdate(deltaTime);

        // Update network entities based on byte amount
        if (TimeUtilities.IsMatchingFrame(_nextSyncableSendRate))
        {
            var lastBytes = NetworkInfo.BytesUp;

            NetworkEntityManager.OnUpdate(deltaTime);

            var byteDifference = NetworkInfo.BytesUp - lastBytes;
            _nextSyncableSendRate = SendRateTable.GetObjectSendRate(byteDifference);
        }

        FusionPlayer.OnUpdate();

        // Update and push all network messages
        VoiceHelper.OnVoiceChatUpdate();

        InternalLayerHelpers.OnUpdateLayer();

        // Update hooks
        MultiplayerHooking.Internal_OnUpdate();

        // Update gamemodes
        GamemodeManager.Internal_OnUpdate();

        // Update delayed events at the very end of the frame
        DelayUtilities.Internal_OnUpdate();
    }

    public override void OnFixedUpdate()
    {
        TimeUtilities.OnEarlyFixedUpdate();

        PhysicsUtilities.OnUpdateTimescale();

        PDController.OnFixedUpdate();

        var deltaTime = TimeUtilities.FixedDeltaTime;

        NetworkPlayerManager.OnFixedUpdate(deltaTime);

        NetworkEntityManager.OnFixedUpdate(deltaTime);

        // Update hooks
        MultiplayerHooking.Internal_OnFixedUpdate();

        // Update gamemodes
        GamemodeManager.Internal_OnFixedUpdate();
    }

    public override void OnLateUpdate()
    {
        // Update players and entity late updates
        float deltaTime = TimeUtilities.DeltaTime;
        NetworkPlayerManager.OnLateUpdate(deltaTime);
        NetworkEntityManager.OnLateUpdate(deltaTime);

        // Flush any left over network messages
        InternalLayerHelpers.OnLateUpdateLayer();

        // Late update hooks
        MultiplayerHooking.Internal_OnLateUpdate();

        // Late update gamemodes
        GamemodeManager.Internal_OnLateUpdate();
    }
}