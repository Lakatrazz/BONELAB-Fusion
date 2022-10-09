using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MelonLoader;

using SLZ.Rig;
using SLZ.Interaction;

using BoneLib;

using UnityEngine;
using SLZ.Marrow.Utilities;
using SLZ.Marrow.Warehouse;
using LabFusion.Utilities;
using UnhollowerRuntimeLib;

namespace LabFusion.Data
{
    public static class RigData
    {
        public static RigManager RigManager { get; private set; }

        public static Hand LeftHand { get; private set; }
        public static Hand RightHand { get; private set; }

        public static BaseController LeftController { get; private set; }
        public static BaseController RightController { get; private set; }

        public static string RigScene { get; private set; }

        public static void OnCacheRigInfo(string sceneName) {
            var rigObject = Player.GetRigManager();

            if (!rigObject) {
                RigScene = null;
                return;
            }

            RigScene = sceneName;

            RigManager = rigObject.GetComponent<RigManager>();
            LeftHand = Player.leftHand;
            RightHand = Player.rightHand;

            LeftController = Player.leftController;
            RightController = Player.rightController;
        }
    }
}
