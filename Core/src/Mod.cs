using System;
using System.Reflection;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Syncables;

using MelonLoader;
using LabFusion.Grabbables;

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
            FusionAssembly = MelonAssembly.Assembly;

            PersistentData.OnPathInitialize();
            FusionMessageHandler.RegisterHandlersFromAssembly(FusionAssembly);
            GrabGroupHandler.RegisterHandlersFromAssembly(FusionAssembly);
            AssetBundleManager.OnLoadBundles();

            OnInitializeNetworking();
        }

        public override void OnLateInitializeMelon() {
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

            SyncManager.OnCleanup();
            RigData.OnCacheRigInfo(sceneName);
            PlayerRep.OnRecreateReps();
        }

        public override void OnUpdate() {
            LevelWarehouseUtilities.OnUpdateLevelLoading();

            RigData.OnRigUpdate();

            InternalLayerHelpers.OnUpdateLayer();
            PlayerRep.OnSyncRep();
        }

        public override void OnFixedUpdate() {
            PlayerRep.OnFixedUpdate();
        }

        public override void OnLateUpdate() {
            InternalLayerHelpers.OnLateUpdateLayer();
        }

        public override void OnGUI() {
            InternalLayerHelpers.OnGUILayer();
        }
    }
}
