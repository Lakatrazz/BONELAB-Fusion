using System;
using System.Reflection;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Syncables;

using MelonLoader;

using LabFusion.Grabbables;
using UnityEngine;
using SLZ.Interaction;
using Il2CppSystem.Collections;
using System.Diagnostics;
using PuppetMasta;

namespace LabFusion
{
    public class FusionMod : MelonMod
    {
        public struct FusionVersion {
            public const byte versionMajor = 0;
            public const byte versionMinor = 0;
            public const short versionPatch = 1;
        }

        public static readonly Version Version = new Version(FusionVersion.versionMajor, FusionVersion.versionMinor, FusionVersion.versionPatch);
        public static FusionMod Instance { get; private set; }
        public static Assembly FusionAssembly { get; private set; }

        public override void OnEarlyInitializeMelon() {
            Instance = this;
            FusionAssembly = Assembly.GetExecutingAssembly();

            PersistentData.OnPathInitialize();
            FusionMessageHandler.RegisterHandlersFromAssembly(FusionAssembly);
            GrabGroupHandler.RegisterHandlersFromAssembly(FusionAssembly);

            PDController.OnMelonInitialize();

            OnInitializeNetworking();
        }

        public override void OnLateInitializeMelon() {
            PatchingUtilities.PatchAll();
            InternalLayerHelpers.OnLateInitializeLayer();
        }

        protected void OnInitializeNetworking() {
            InternalLayerHelpers.SetLayer(new SteamNetworkLayer());
        }

        public override void OnDeinitializeMelon() {
            InternalLayerHelpers.OnCleanupLayer();
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
            
            // Create player reps
            PlayerRep.OnRecreateReps();
        }

        public override void OnUpdate() {
            // Reset byte counts
            NetworkInfo.BytesDown = 0;
            NetworkInfo.BytesUp = 0;

            // Update the jank level loading check
            LevelWarehouseUtilities.OnUpdateLevelLoading();

            // Store rig info/update avatars
            RigData.OnRigUpdate();

            // Send world messages every other frame
            if (Time.frameCount % 2 == 0) {
                PlayerRep.OnSyncRep();
                SyncManager.OnUpdate();
                PhysicsUtilities.OnSendPhysicsInformation();
            }

            // Update and push all network messages
            InternalLayerHelpers.OnUpdateLayer();
        }

        public override void OnFixedUpdate() {
            PDController.OnFixedUpdate();
            PlayerRep.OnFixedUpdate();
            SyncManager.OnFixedUpdate();
        }

        public override void OnLateUpdate() {
            // Update stuff like nametags
            PlayerRep.OnLateUpdate();

            // Flush any left over network messages
            InternalLayerHelpers.OnLateUpdateLayer();
        }

        public override void OnGUI() {
            InternalLayerHelpers.OnGUILayer();
        }
    }
}
