using HarmonyLib;

using LabFusion.Network;

using UnityEngine;

using LabFusion.Utilities;

using SLZ.Marrow.SceneStreaming;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ChunkTrigger), nameof(ChunkTrigger.OnTriggerEnter))]
    public static class ChunkEnterPatch
    {
        public static bool Prefix(Collider other)
        {
            if (other.CompareTag("Player") && NetworkInfo.HasServer)
            {
                return TriggerUtilities.IsMainRig(other);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ChunkTrigger), nameof(ChunkTrigger.OnTriggerExit))]
    public static class ChunkExitPatch
    {
        public static bool Prefix(Collider other)
        {
            if (other.CompareTag("Player") && NetworkInfo.HasServer)
            {
                return TriggerUtilities.IsMainRig(other);
            }

            return true;
        }
    }
}

