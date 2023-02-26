using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Data;
using LabFusion.Network;

using SLZ.Bonelab;

using UnityEngine;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(JimmyDoorController))]
    public static class JimmyDoorControllerPatches {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(JimmyDoorController.OnTriggerEnter))]
        public static void OnTriggerEnter(JimmyDoorController __instance, Collider other) {
            if (NetworkInfo.HasServer) {
                // Make sure the rigmanager is ours so we get yoinked by jimmy
                __instance.rM = RigData.RigReferences.RigManager;
            }
        }
    }
}
