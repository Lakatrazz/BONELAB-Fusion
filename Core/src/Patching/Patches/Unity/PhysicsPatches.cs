using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Network;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(Physics))]
    public static class PhysicsPatches {
        [HarmonyPatch(nameof(Physics.gravity), MethodType.Setter)]
        [HarmonyPrefix]
        public static bool SetGravity(Vector3 value) {
            if (NetworkInfo.HasServer && !NetworkInfo.IsServer && !PhysicsUtilities.CanModifyGravity) {
                return false;
            }

            return true;
        }
    }
}
