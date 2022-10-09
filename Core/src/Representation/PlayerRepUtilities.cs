using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SLZ.Rig;

using UnhollowerRuntimeLib;

using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Data;

namespace LabFusion.Representation {
    public static class PlayerRepUtilities {
        public static RigManager CreateNewRig() {
            var go = GameObject.Instantiate(AssetBundleManager.PlayerRepBundle.LoadAsset(ResourcePaths.PlayerRepName, Il2CppType.Of<GameObject>())).Cast<GameObject>();
            var manager = go.GetComponent<RigManager>();

            manager.openControllerRig.leftController = manager.openControllerRig.leftController.gameObject.AddComponent<NullController>();
            manager.openControllerRig.leftController.handedness = SLZ.Handedness.LEFT;

            manager.openControllerRig.rightController = manager.openControllerRig.rightController.gameObject.AddComponent<NullController>();
            manager.openControllerRig.rightController.handedness = SLZ.Handedness.RIGHT;

            return manager;
        }
    }
}
