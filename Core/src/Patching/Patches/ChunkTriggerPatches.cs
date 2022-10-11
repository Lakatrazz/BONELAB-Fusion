using HarmonyLib;

using System;

using LabFusion.Network;
using LabFusion.Extensions;

using SLZ.Zones;

using UnityEngine;

using System.Collections.Generic;

using LabFusion.Utilities;

using SLZ.AI;
using SLZ.Rig;

using LabFusion.Data;

using SLZ.Marrow.SceneStreaming;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ChunkTrigger), "OnTriggerEnter")]
    public static class ChunkEnterPatch
    {
        public static bool Prefix(ChunkTrigger __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                return TriggerUtilities.IsMainRig(other);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ChunkTrigger), "OnTriggerExit")]
    public static class ChunkExitPatch
    {
        public static bool Prefix(ChunkTrigger __instance, Collider other)
        {
            if (other.CompareTag("Player"))
            {
                return TriggerUtilities.IsMainRig(other);
            }

            return true;
        }
    }
}

