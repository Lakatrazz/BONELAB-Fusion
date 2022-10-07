using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Utilities;

using MelonLoader;

using UnhollowerRuntimeLib;

using UnityEngine;

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

        public override void OnEarlyInitializeMelon() {
            Instance = this;
            FusionAssembly = MelonAssembly.Assembly;

            PersistentData.OnPathInitialize();
            FusionMessageHandler.RegisterHandlersFromAssembly(FusionAssembly);

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

        public override void OnSceneWasInitialized(int buildIndex, string sceneName) {
            RigData.OnCacheRigInfo();
        }

        public override void OnUpdate() {
            if (CurrentNetworkLayer != null) {
                CurrentNetworkLayer.OnUpdateLayer();
            }
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
