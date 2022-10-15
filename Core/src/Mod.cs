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
using static SLZ.UI.SceneAmmoUI;

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

        private static string _prevLevelBarcode = null;

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

        public static void OnUpdateLevelLoading() {
            if (LevelWarehouseUtilities.IsLoadDone()) {
                var code = LevelWarehouseUtilities.GetCurrentLevel().Barcode;

                if (_prevLevelBarcode != code) {
                    OnMainSceneInitialized();
                    _prevLevelBarcode = code;
                }
            }
        }

        public static void OnMainSceneInitialized() {
            string sceneName = LevelWarehouseUtilities.GetCurrentLevel().Title;

#if DEBUG
            FusionLogger.Log($"Main scene {sceneName} was initialized.");
#endif

            RigData.OnCacheRigInfo(sceneName);
            PlayerRep.OnRecreateReps();
        }

        public override void OnUpdate() {
            OnUpdateLevelLoading();

            RigData.OnRigUpdate();

            if (CurrentNetworkLayer != null) {
                CurrentNetworkLayer.OnUpdateLayer();
                PlayerRep.OnSyncRep();
            }
        }

        public override void OnFixedUpdate() {
            PlayerRep.OnFixedUpdate();
        }

        public override void OnLateUpdate() {
            if (CurrentNetworkLayer != null) {
                CurrentNetworkLayer.OnLateUpdateLayer();
            }
        }

        public override void OnGUI() {
            if (CurrentNetworkLayer != null) {
                CurrentNetworkLayer.OnGUILayer();
            }
        }
    }
}
