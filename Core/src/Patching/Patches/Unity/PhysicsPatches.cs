using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Utilities;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(Physics))]
    public static class PhysicsPatches
    {
        [HarmonyPatch(nameof(Physics.gravity), MethodType.Setter)]
        [HarmonyPrefix]
        public static bool SetGravityPrefix(Vector3 value)
        {
            if (NetworkInfo.HasServer && !NetworkInfo.IsServer && !PhysicsUtilities.CanModifyGravity)
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(Physics.gravity), MethodType.Setter)]
        [HarmonyPostfix]
        public static void SetGravityPostfix(Vector3 value)
        {
            var output = value.ToSystemVector3();

            if (NetworkInfo.IsServer)
            {
                PhysicsUtilities.SendGravity(output);
            }

            PhysicsUtilities.Gravity = output;
        }
    }
}
