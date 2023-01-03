using System;
using System.Reflection;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Syncables;
using LabFusion.Grabbables;
using LabFusion.SDK.Modules;

using MelonLoader;

using UnityEngine;
using LabFusion.Extensions;

namespace LabFusion
{
    public struct FusionVersion
    {
        public const byte versionMajor = 0;
        public const byte versionMinor = 0;
        public const short versionPatch = 1;
    }

    public class FusionMod : MelonMod {
        public const string Name = "LabFusion";
        public const string Author = "Lakatrazz";
        public static readonly Version Version = new Version(FusionVersion.versionMajor, FusionVersion.versionMinor, FusionVersion.versionPatch);

        public static FusionMod Instance { get; private set; }
        public static Assembly FusionAssembly { get; private set; }

        private static int _nextSyncableSendRate = 1;

        public override void OnEarlyInitializeMelon() {
            Instance = this;
            FusionAssembly = Assembly.GetExecutingAssembly();

            PersistentData.OnPathInitialize();
            FusionMessageHandler.RegisterHandlersFromAssembly(FusionAssembly);
            GrabGroupHandler.RegisterHandlersFromAssembly(FusionAssembly);
            PropExtenderManager.RegisterExtendersFromAssembly(FusionAssembly);

            PDController.OnMelonInitialize();

            ModuleHandler.Internal_HookAssemblies();

            OnInitializeNetworking();
        }

        public override void OnLateInitializeMelon() {
            PatchingUtilities.PatchAll();
            InternalLayerHelpers.OnLateInitializeLayer();
            PersistentAssetCreator.OnMelonInitialize();

            FusionPreferences.OnInitializePreferences();
        }

        protected void OnInitializeNetworking() {
            InternalLayerHelpers.SetLayer(new SteamNetworkLayer());
        }

        public override void OnDeinitializeMelon() {
            InternalLayerHelpers.OnCleanupLayer();
            ModuleHandler.Internal_UnhookAssemblies();
        }

        public override void OnPreferencesLoaded() {
            FusionPreferences.OnPreferencesLoaded();
        }

        public static void OnMainSceneInitialized() {
            string sceneName = LevelWarehouseUtilities.GetCurrentLevel().Title;
            
#if DEBUG
            FusionLogger.Log($"Main scene {sceneName} was initialized.");
#endif
            // Cache info
            SyncManager.OnCleanup();
            RigData.OnCacheRigInfo();

            // Level info
            ArenaData.OnCacheArenaInfo();
            DescentData.OnCacheDescentInfo();
            HubData.OnCacheHubInfo();
            MagmaGateData.OnCacheMagmaGateInfo();
            
            // Create player reps
            PlayerRep.OnRecreateReps();

            // Update hooks
            HookingUtilities.Internal_OnMainSceneInitialized();
        }

        public static void OnMainSceneInitializeDelayed() {
            // Make sure the rig exists
            if (RigData.RigReferences.RigManager.IsNOC())
                return;

            // Force enable radial menu
            RigData.RigReferences.RigManager.bodyVitals.quickmenuEnabled = true;
            RigData.RigReferences.RigManager.openControllerRig.quickmenuEnabled = true;
        }

        public override void OnUpdate() {
            // Reset byte counts
            NetworkInfo.BytesDown = 0;
            NetworkInfo.BytesUp = 0;

            // Update the jank level loading check
            LevelWarehouseUtilities.OnUpdateLevelLoading();

            // Store rig info/update avatars
            RigData.OnRigUpdate();

            // Send players based on player count
            int playerSendRate = SendRateTable.GetPlayerSendRate();
            if (Time.frameCount % playerSendRate == 0) {
                PlayerRep.OnSyncRep();
            }

            // Send syncables based on byte amount
            if (Time.frameCount % _nextSyncableSendRate == 0) {
                var lastBytes = NetworkInfo.BytesUp;

                SyncManager.OnUpdate();

                var byteDifference = NetworkInfo.BytesUp - lastBytes;
                _nextSyncableSendRate = SendRateTable.GetObjectSendRate(byteDifference);
            }

            // Send gravity every 40 frames
            if (Time.frameCount % 40 == 0) {
                PhysicsUtilities.OnSendPhysicsInformation();
            }

            // Update and push all network messages
            InternalLayerHelpers.OnUpdateLayer();

            // Update hooks
            HookingUtilities.Internal_OnUpdate();
        }

        public override void OnFixedUpdate() {
            PDController.OnFixedUpdate();
            PlayerRep.OnFixedUpdate();
            SyncManager.OnFixedUpdate();

            // Update hooks
            HookingUtilities.Internal_OnFixedUpdate();
        }

        public override void OnLateUpdate() {
            // Update stuff like nametags
            PlayerRep.OnLateUpdate();

            // Flush any left over network messages
            InternalLayerHelpers.OnLateUpdateLayer();

            // Update hooks
            HookingUtilities.Internal_OnLateUpdate();
        }

        public override void OnGUI() {
            InternalLayerHelpers.OnGUILayer();
        }
    }
}
