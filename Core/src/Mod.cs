using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;

using MelonLoader;

using SLZ.Marrow.SceneStreaming;

using UnhollowerRuntimeLib;

using UnityEngine;
using UnityEngine.SceneManagement;
using SLZ.Rig;

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
        public static NetworkLayer CurrentNetworkLayer { get; private set; }

        private static float _prevTimeScale;

        public override void OnEarlyInitializeMelon() {
            Instance = this;
            FusionAssembly = MelonAssembly.Assembly;

            PersistentData.OnPathInitialize();
            FusionMessageHandler.RegisterHandlersFromAssembly(FusionAssembly);
            AssetBundleManager.OnLoadBundles();

            OnInitializeNetworking();
        }

        public override void OnLateInitializeMelon() {
            if (CurrentNetworkLayer != null)
                CurrentNetworkLayer.OnLateInitializeLayer();
        }

        protected void OnInitializeNetworking() {
            CurrentNetworkLayer = new SteamNetworkLayer();
            CurrentNetworkLayer.OnInitializeLayer();
        }

        public override void OnDeinitializeMelon() {
            if (CurrentNetworkLayer != null)
                CurrentNetworkLayer.OnCleanupLayer();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            if (!RigData.RigManager)
                RigData.OnCacheRigInfo(sceneName);
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
            if (sceneName == RigData.RigScene)
                OnMainSceneInitialized(buildIndex, sceneName);
        }

        public static void OnMainSceneInitialized(int buildIndex, string sceneName) {
#if DEBUG
            FusionLogger.Log($"Main scene {sceneName} was initialized.");
#endif

            PlayerRep.OnRecreateReps(true);

            //PlayerRepUtilities.CreateNewRig();
        }

        public override void OnUpdate() {
            PlayerRep.OnVerifyReps();

            if (CurrentNetworkLayer != null) {
                CurrentNetworkLayer.OnUpdateLayer();
                PlayerRep.OnSyncRep();
            }
        }

        public override void OnLateUpdate() {
            if (CurrentNetworkLayer != null) {
                CurrentNetworkLayer.OnLateUpdateLayer();
            }

            // Temp fix for 0, 0, 0 player scale bug! Find a better one in the future that doesn't break third person!
            if (Time.timeScale > 0f && _prevTimeScale <= 0f && RigData.RigManager) {
                RigData.RigManager.bodyVitals.CalibratePlayerBodyScale();
            }

            _prevTimeScale = Time.timeScale;
        }

        public override void OnGUI() {
            if (CurrentNetworkLayer != null) {
                CurrentNetworkLayer.OnGUILayer();
            }
        }
    }
}
