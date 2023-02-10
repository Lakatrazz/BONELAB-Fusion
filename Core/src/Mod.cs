using System;
using System.Reflection;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Syncables;
using LabFusion.Grabbables;
using LabFusion.SDK.Modules;
using LabFusion.Extensions;
using LabFusion.Preferences;

#if DEBUG
using LabFusion.Debugging;
#endif

using MelonLoader;

using UnityEngine;

using BoneLib;

using LabFusion.Senders;

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

        /// <summary>
        /// The desired networking layer. Swap this out to change the networking system.
        /// </summary>
        public static Type ActiveNetworkingType { get; private set; } = typeof(SteamNetworkLayer);

        public static FusionMod Instance { get; private set; }
        public static Assembly FusionAssembly { get; private set; }

        private static int _nextSyncableSendRate = 1;

        public override void OnEarlyInitializeMelon() {
            Instance = this;
            FusionAssembly = Assembly.GetExecutingAssembly();
            
            // Initialize data and hooks
            BytePool.PopulateInitial();
            PersistentData.OnPathInitialize();
            PDController.OnInitializeMelon();
            ModuleHandler.Internal_HookAssemblies();
            PlayerAdditionsHelper.OnInitializeMelon();
        }

        public override void OnInitializeMelon() {
            // Pull files
            FusionFileLoader.OnInitializeMelon();

            // Register our base handlers
            FusionMessageHandler.RegisterHandlersFromAssembly(FusionAssembly);
            GrabGroupHandler.RegisterHandlersFromAssembly(FusionAssembly);
            PropExtenderManager.RegisterExtendersFromAssembly(FusionAssembly);

            // Finally, initialize the network layer
            OnInitializeNetworking();
        }

        public override void OnLateInitializeMelon() {
            PatchingUtilities.PatchAll();
            InternalLayerHelpers.OnLateInitializeLayer();
            PersistentAssetCreator.OnMelonInitialize();

            FusionPreferences.OnInitializePreferences();

            Hooking.OnLevelInitialized += OnBonelibLevelLoaded;
        }

        public void OnBonelibLevelLoaded(LevelInfo info) {
            LevelData.OnSceneAwake();
        }

        protected void OnInitializeNetworking() {
            // If a layer is already set, don't initialize
            if (NetworkInfo.CurrentNetworkLayer != null) {
                FusionLogger.Warn("Cannot initialize new network layer because a previous one is active!");
                return;
            }

            // Validate the type
            if (!ActiveNetworkingType.IsSubclassOf(typeof(NetworkLayer))) {
                FusionLogger.Error("The target network layer type is invalid!");
                return;
            }

            // Create the network layer based on the selected type
            // Then, set the layer
            var layer = Activator.CreateInstance(ActiveNetworkingType) as NetworkLayer;
            InternalLayerHelpers.SetLayer(layer);
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
            // Fix random static grips in the scene
            StaticGripFixer.OnMainSceneInitialized();

            // Cache info
            SyncManager.OnCleanup();
            RigData.OnCacheRigInfo();

            // Create player reps
            PlayerRep.OnRecreateReps();

            // Update level data
            LevelData.OnMainSceneInitialized();

            // Update hooks
            MultiplayerHooking.Internal_OnMainSceneInitialized();
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

            // Update notifications
            FusionNotifier.OnUpdate();

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

            // Update timescale
            if (NetworkInfo.HasServer) {
                var mode = FusionPreferences.TimeScaleMode;

                switch (mode) {
                    case TimeScaleMode.DISABLED:
                        Time.timeScale = 1f;
                        break;
                    case TimeScaleMode.LOW_GRAVITY:
                        Time.timeScale = 1f;

                        var rm = RigData.RigReferences.RigManager;
                        if (!rm.IsNOC()) {
                            var controlTime = RigData.RigReferences.RigManager.openControllerRig.globalTimeControl;
                            float mult = 1f - (1f / controlTime.cur_intensity);
                            if (float.IsNaN(mult) || mult == 0f || float.IsPositiveInfinity(mult) || float.IsNegativeInfinity(mult))
                                break;

                            Vector3 force = -Physics.gravity * mult;

                            if (RigData.RigReferences.RigRigidbodies == null)
                                RigData.RigReferences.GetRigidbodies();

                            var rbs = RigData.RigReferences.RigRigidbodies;

                            foreach (var rb in rbs) {
                                if (rb.useGravity) {
                                    rb.AddForce(force, ForceMode.Acceleration);
                                }
                            }
                        }

                        break;
                }
            }

            // Update reps
            PlayerRep.OnUpdate();

            // Update and push all network messages
            InternalLayerHelpers.OnUpdateLayer();

            // Update hooks
            MultiplayerHooking.Internal_OnUpdate();
        }

        public override void OnFixedUpdate() {
            PDController.OnFixedUpdate();
            PlayerRep.OnFixedUpdate();
            SyncManager.OnFixedUpdate();

            // Update hooks
            MultiplayerHooking.Internal_OnFixedUpdate();
        }

        public override void OnLateUpdate() {
            // Update stuff like nametags
            PlayerRep.OnLateUpdate();

            // Flush any left over network messages
            InternalLayerHelpers.OnLateUpdateLayer();

            // Update hooks
            MultiplayerHooking.Internal_OnLateUpdate();
        }

        public override void OnGUI() {
            InternalLayerHelpers.OnGUILayer();
        }
    }
}
