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
    [HarmonyPatch(typeof(Physics), nameof(Physics.gravity), MethodType.Setter)]
    public class GravityPatch {
        public static bool Prefix(Vector3 value) {
            if (NetworkInfo.HasServer && !NetworkInfo.IsServer && !PhysicsUtilities.CanModifyGravity) {
                return false;
            }

            return true;
        }
    }
}
