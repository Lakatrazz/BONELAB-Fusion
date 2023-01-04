using HarmonyLib;

using System;
using System.Reflection;

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
    [HarmonyPatch(typeof(SceneLoadQueue))]
    public static class SceneLoadQueuePatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SceneLoadQueue.AddUnload))]
        public static bool AddUnload(string address) {
            if (NetworkInfo.HasServer) {
                var loader = SceneStreamer.Session.ChunkLoader;

                foreach (var chunk in TriggerUtilities.ChunkCount.Keys) {
                    foreach (var layer in chunk.sceneLayers) {
                        if (layer.AssetGUID == address && loader._activeChunks.Contains(chunk)) {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ChunkTrigger))]
    public static class ChunkTriggerPatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ChunkTrigger.OnUnload))]
        public static bool OnUnload(ChunkTrigger __instance) {
            if (!LevelWarehouseUtilities.IsDelayedLoadDone())
                return true;

            if (NetworkInfo.HasServer) {
                var loader = SceneStreamer.Session.ChunkLoader;
                var chunk = __instance.chunk;
                bool canUnload = true;

                if (!TriggerUtilities.CanUnload(chunk)) {
                    canUnload = false;

                    if (loader._chunksToUnload.Contains(chunk)) {
                        loader._chunksToUnload.Remove(chunk);
                        loader._activeChunks.Add(chunk);
                    }
                }

                return canUnload;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ChunkTrigger), "OnTriggerEnter")]
    public static class ChunkEnterPatch
    {
        public static bool Prefix(ChunkTrigger __instance, Collider other)
        {
            if (other.CompareTag("Player") && NetworkInfo.HasServer) {
                TriggerUtilities.Increment(__instance.chunk);
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ChunkTrigger), "OnTriggerExit")]
    public static class ChunkExitPatch
    {
        public static bool Prefix(ChunkTrigger __instance, Collider other)
        {
            if (other.CompareTag("Player") && NetworkInfo.HasServer) {
                TriggerUtilities.Decrement(__instance.chunk);
            }

            return true;
        }
    }
}

