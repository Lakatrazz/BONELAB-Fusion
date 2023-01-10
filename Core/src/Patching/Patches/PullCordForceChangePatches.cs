using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Network;
using LabFusion.Utilities;
using SLZ.Props;

using UnityEngine;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(PullCordForceChange))]
    public static class PullCordForceChangePatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(PullCordForceChange.OnTriggerEnter))]
        public static bool OnTriggerEnter(Collider other) {
            if (NetworkInfo.HasServer && other.CompareTag("Player")) {
                return TriggerUtilities.IsMainRig(other);
            }

            return true;
        }
    }
}
