using System.Reflection;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Preferences;
using LabFusion.SDK.Gamemodes;
using LabFusion.SDK.Points;
using LabFusion.SDK.Achievements;
using LabFusion.Voice;
using LabFusion.SDK.Lobbies;

#if DEBUG
using LabFusion.Debugging;
#endif

using MelonLoader;

using BoneLib;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Rig;

using ModuleHandler = LabFusion.SDK.Modules.ModuleHandler;

using UnityEngine;
using Il2CppSLZ.Marrow.Warehouse;
using LabFusion.SDK.Cosmetics;
using LabFusion.Entities;

namespace LabFusion;

public struct FusionVersion
{
    public const byte versionMajor = 1;
    public const byte versionMinor = 7;
    public const short versionPatch = 0;
}

public class FusionMod : MelonMod
{
    public const string Name = "LabFusion";
    public const string Author = "Lakatrazz";
    public static readonly Version Version = new(FusionVersion.versionMajor, FusionVersion.versionMinor, FusionVersion.versionPatch);

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

        // Prepare the data path for writing files
        PersistentData.OnPathInitialize();

        // Load APIs
        SteamAPILoader.OnLoadSteamAPI();

        // Initialize data and hooks
        ByteRetriever.PopulateInitial();
        PDController.OnInitializeMelon();
        ModuleHandler.Internal_HookAssemblies();
        GamemodeRegistration.Internal_HookAssemblies();
        PointItemManager.Internal_HookAssemblies();

        VoteKickHelper.Internal_OnInitializeMelon();
    }

    public override void OnInitializeMelon()
    {
        // Prepare the bonemenu category
        FusionPreferences.OnPrepareBoneMenuCategory();

        // Pull files
        FusionFileLoader.OnInitializeMelon();

        // Load assetbundles
        FusionBundleLoader.OnBundleLoad();

        // Register our base handlers
        LevelDataHandler.OnInitializeMelon();
        FusionMessageHandler.RegisterHandlersFromAssembly(FusionAssembly);
        GrabGroupHandler.RegisterHandlersFromAssembly(FusionAssembly);
        NetworkLayer.RegisterLayersFromAssembly(FusionAssembly);
        GamemodeRegistration.LoadGamemodes(FusionAssembly);
        //PointItemManager.LoadItems(FusionAssembly);
        AchievementManager.LoadAchievements(FusionAssembly);

        EntityComponentManager.RegisterComponentsFromAssembly(FusionAssembly);

        LobbyFilterManager.LoadBuiltInFilters();

        NetworkEntityManager.OnInitializeManager();
        NetworkPlayerManager.OnInitializeManager();

        FusionPopupManager.OnInitializeMelon();

        // Hook into asset warehouse
        var onReady = () =>
        {
            CosmeticLoader.LoadAllCosmetics();
        };
        AssetWarehouse.OnReady(onReady);

        // Create prefs
        FusionPreferences.OnInitializePreferences();

        // Initialize level loading
        FusionSceneManager.Internal_OnInitializeMelon();

        // Finally, initialize the network layer
        OnInitializeNetworking();

#if DEBUG
        FusionUnityLogger.OnInitializeMelon();
#endif
    }

    public override void OnLateInitializeMelon()
    {
        InternalLayerHelpers.OnLateInitializeLayer();
        PersistentAssetCreator.OnLateInitializeMelon();
        PlayerAdditionsHelper.OnInitializeMelon();

        FusionPreferences.OnCreateBoneMenu();

        // Check if the auto updater is installed
        _hasAutoUpdater = MelonPlugin.RegisteredMelons.Any((p) => p.Info.Name.Contains("LabFusion Updater"));

        if (!_hasAutoUpdater && !HelperMethods.IsAndroid())
        {
            FusionNotifier.Send(new FusionNotification()
            {
                isMenuItem = false,
                isPopup = true,
                message = "You do not have the Fusion AutoUpdater installed in your plugins folder!" +
                "\nIt is recommended to install it in order to stay up to date.",
                type = NotificationType.WARNING,
            });

#if DEBUG
            FusionLogger.Warn("The player does not have the auto updater installed.");
#endif
        }
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

        VoteKickHelper.Internal_OnDeinitializeMelon();

        // Backup files
        FusionFileLoader.OnDeinitializeMelon();

        // Unhook assembly loads
        ModuleHandler.Internal_UnhookAssemblies();
        GamemodeRegistration.Internal_UnhookAssemblies();
        PointItemManager.Internal_UnhookAssemblies();

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

        // Stop the current gamemode
        if (NetworkInfo.IsServer && Gamemode.ActiveGamemode != null && Gamemode.ActiveGamemode.AutoStopOnSceneLoad)
            Gamemode.ActiveGamemode.StopGamemode();
    }

    public static void OnMainSceneInitializeDelayed()
    {
        // Make sure the rig exists
        if (!RigData.HasPlayer)
            return;

        // Force enable radial menu
        RigData.RigReferences.RigManager.GetComponentInChildren<BodyVitals>().quickmenuEnabled = true;
        RigData.RigReferences.RigManager.ControllerRig.TryCast<OpenControllerRig>().quickmenuEnabled = true;
    }

    public override void OnUpdate()
    {
        // Reset byte counts
        NetworkInfo.BytesDown = 0;
        NetworkInfo.BytesUp = 0;

        // Update Time before running any functions
        TimeUtilities.OnEarlyUpdate();

        // Update threaded events
        ThreadingUtilities.Internal_OnUpdate();

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

        // Update hooks
        MultiplayerHooking.Internal_OnLateUpdate();

        // Update gamemodes
        GamemodeManager.Internal_OnLateUpdate();
    }

    public override void OnGUI()
    {
        InternalLayerHelpers.OnGUILayer();

#if DEBUG
        var emptyOptions = Array.Empty<GUILayoutOption>();

        GUILayout.Label($"Bytes Up: {NetworkInfo.BytesUp}", emptyOptions);
        GUILayout.Label($"Bytes Down: {NetworkInfo.BytesDown}", emptyOptions);

        GUILayout.Label($"Network Entity Count: {NetworkEntityManager.IdManager.RegisteredEntities.EntityIdLookup.Count}", emptyOptions);
#endif
    }
}