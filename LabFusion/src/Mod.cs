using System.Reflection;

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
using LabFusion.SDK;
using LabFusion.RPC;
using LabFusion.UI.Popups;
using LabFusion.Safety;

#if DEBUG
using LabFusion.Debugging;
#endif

using MelonLoader;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Warehouse;
using Il2CppSLZ.Marrow;

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
    public const byte VersionMinor = 12;
    public const short VersionPatch = 2;

    public const string VersionString = "1.12.2";
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
        PDController.OnInitializeMelon();
        PointItemManager.HookEvents();
        RpcManager.OnInitialize();
    }

    public override void OnInitializeMelon()
    {
        // Pull files
        FusionFileLoader.OnInitializeMelon();
        ListLoader.OnInitializeMelon();

        // Initialize player
        LocalPlayer.OnInitializeMelon();
        VoiceSourceManager.OnInitializeMelon();

        // Register base modules
        InitializeBaseModules();

        // Register our base handlers
        NativeMessageHandler.RegisterHandlersFromAssembly(FusionAssembly);
        GrabGroupHandler.RegisterHandlersFromAssembly(FusionAssembly);
        NetworkLayer.RegisterLayersFromAssembly(FusionAssembly);
        GamemodeRegistration.LoadGamemodes(FusionAssembly);
        PointItemManager.LoadItems(FusionAssembly);
        AchievementManager.LoadAchievements(FusionAssembly);
        RpcManager.LoadRpcs(FusionAssembly);

        EntityComponentManager.LoadComponents(FusionAssembly);

        LobbyFilterManager.LoadBuiltInFilters();

        NetworkEntityManager.OnInitializeManager();
        NetworkPlayerManager.OnInitializeManager();

        PopupManager.OnInitializeMelon();

        GamemodeManager.OnInitializeMelon();
        GamemodeConditionsChecker.OnInitializeMelon();
        GamemodeRoundManager.OnInitializeMelon();

        // Hook into asset warehouse
        var onReady = () =>
        {
            CosmeticLoader.OnAssetWarehouseReady();
        };
        AssetWarehouse.OnReady(onReady);

        // Create prefs
        FusionPreferences.OnInitializePreferences();

        FusionPermissions.OnInitializeMelon();

        LobbyInfoManager.OnInitialize();

        MenuCreator.OnInitializeMelon();

        // Initialize level loading
        FusionSceneManager.Internal_OnInitializeMelon();
        MultiplayerHooking.OnLoadingBegin += OnLoadingBegin;
        NetworkSceneManager.OnInitializeMelon();

        // Initialize the networking manager
        NetworkLayerManager.OnInitializeMelon();

#if DEBUG
        FusionUnityLogger.OnInitializeMelon();
#endif
    }

    private static void InitializeBaseModules()
    {
        ModuleManager.RegisterModule<SDKModule>();
        ModuleManager.RegisterModule<MarrowModule>();
        ModuleManager.RegisterModule<BonelabModule>();
    }

    public override void OnLateInitializeMelon()
    {
        PersistentAssetCreator.OnLateInitializeMelon();
        PlayerAdditionsHelper.OnInitializeMelon();

#if RELEASE
        // Check if the auto updater is installed
        _hasAutoUpdater = MelonPlugin.RegisteredMelons.Any((p) => p.Info.Name.Contains("LabFusion Updater"));

        if (!_hasAutoUpdater)
        {
            Notifier.Send(new Notification()
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

    public override void OnDeinitializeMelon()
    {
        // Log out of the current layer
        NetworkLayerManager.LogOut();

        // Backup files
        FusionFileLoader.OnDeinitializeMelon();

        // Unhook assembly loads
        PointItemManager.UnhookEvents();

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
        MultiplayerHooking.InvokeOnMainSceneInitialized();

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

    private void OnLoadingBegin()
    {
        ModIOThumbnailDownloader.ClearCache();
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
        PopupManager.OnUpdate();

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
        MultiplayerHooking.InvokeOnUpdate();

        // Update gamemodes
        GamemodeManager.OnUpdate();

        // Update delayed events at the very end of the frame
        DelayUtilities.OnProcessDelays();
    }

    public override void OnFixedUpdate()
    {
        TimeUtilities.OnEarlyFixedUpdate();

        LocalPlayer.OnFixedUpdate();

        PDController.OnFixedUpdate();

        var deltaTime = TimeUtilities.FixedDeltaTime;

        NetworkPlayerManager.OnFixedUpdate(deltaTime);

        NetworkEntityManager.OnFixedUpdate(deltaTime);

        // Update hooks
        MultiplayerHooking.InvokeOnFixedUpdate();

        // Update gamemodes
        GamemodeManager.OnFixedUpdate();
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
        MultiplayerHooking.InvokeOnLateUpdate();

        // Late update gamemodes
        GamemodeManager.OnLateUpdate();
    }
}