using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SLZ.Rig;

using UnhollowerRuntimeLib;

using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Syncables;
using LabFusion.Extensions;

using SLZ;
using SLZ.Interaction;
using MelonLoader;
using LabFusion.Grabbables;

namespace LabFusion.Representation {
    public static class PlayerRepUtilities {
        public const int TransformSyncCount = 3;

        public const string PolyBlankBarcode = "c3534c5a-94b2-40a4-912a-24a8506f6c79";

        public static bool FindAttachedPlayerRep(Grip grip, out PlayerRep rep) {
            rep = null;

            if (grip == null)
                return false;

            var rig = RigManager.Cache.Get(grip.transform.root.gameObject);
            if (rig && PlayerRep.Managers.ContainsKey(rig))
            {
                rep = PlayerRep.Managers[rig];
                return true;
            }
            else {
                return false;
            }
        }

        public static RigManager CreateNewRig() {
            var go = GameObject.Instantiate(AssetBundleManager.PlayerRepBundle.LoadAsset(ResourcePaths.PlayerRepName, Il2CppType.Of<GameObject>())).Cast<GameObject>();

            if (RigData.RigReferences.RigManager) {
                go.transform.position = RigData.RigSpawn;
                go.transform.rotation = RigData.RigSpawnRot;
            }

            return go.GetComponent<RigManager>();
        }

        public static void FillTransformArray(ref Transform[] array, RigManager manager) {
            var rig = manager.openControllerRig;

            array[0] = rig.m_head;
            array[1] = rig.m_handLf;
            array[2] = rig.m_handRt;
        }
    }
}
