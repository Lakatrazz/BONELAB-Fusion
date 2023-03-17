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

using IL2ChunkList = Il2CppSystem.Collections.Generic.List<SLZ.Marrow.SceneStreaming.Chunk>;
using SLZ.Marrow.Pool;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(SceneLoadQueue))]
    public static class SceneLoadQueuePatches {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(SceneLoadQueue.AddUnload))]
        public static bool AddUnload(string address) {
            if (NetworkInfo.HasServer) {
                var loader = SceneStreamer.Session.ChunkLoader;

                // Ew, nested foreach loops
                foreach (var pair in TriggerUtilities.PlayerChunks) {
                    foreach (var chunk in pair.Value) {
                        foreach (var layer in chunk.sceneLayers) {
                            if (layer.AssetGUID == address && loader._activeChunks.Has(chunk)) {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(ChunkTrigger))]
    public static class ChunkTriggerPatches {
        public static void Patch() {
            var harmonyInstance = FusionMod.Instance.HarmonyInstance;

            // Get chunk trigger method info
            var triggerEnterMethod = typeof(ChunkTrigger).GetMethod(nameof(ChunkTrigger.OnTriggerEnter), AccessTools.all);

            // Get patches
            var triggerEnterPrefix = new HarmonyMethod(typeof(ChunkTriggerPatches).GetMethod(nameof(OnTriggerEnterPrefix), AccessTools.all));
            var triggerEnterPostfix = new HarmonyMethod(typeof(ChunkTriggerPatches).GetMethod(nameof(OnTriggerEnterPostfix), AccessTools.all));

            // Now actually patch the methods
            harmonyInstance.Patch(triggerEnterMethod, triggerEnterPrefix, triggerEnterPostfix);
        }

        // ChunkEnter patches
        private static bool OnTriggerEnterPrefix(ChunkTrigger __instance, Collider other, ref IL2ChunkList __state)
        {
            if (other.CompareTag("Player") && NetworkInfo.HasServer)
            {
                if (HubData.IsInHub())
                    TriggerUtilities.AddChunk(other, __instance.chunk);
                else
                    TriggerUtilities.SetChunk(other, __instance.chunk);

                var loader = SceneStreamer.Session.ChunkLoader;
                __state = loader._occupiedChunks;

                var playerChunks = TriggerUtilities.GetChunks(other);
                foreach (var playerChunk in playerChunks)
                    __state.Add(playerChunk);

                loader._occupiedChunks.Clear();
            }

            return true;
        }

        private static void OnTriggerEnterPostfix(ChunkTrigger __instance, Collider other, IL2ChunkList __state)
        {
            if (__state == null)
                return;

            var loader = SceneStreamer.Session.ChunkLoader;
            foreach (var chunk in __state)
            {
                if (!loader._occupiedChunks.Has(chunk))
                    loader._occupiedChunks.Add(chunk);
            }
        }

        // Automatic patches
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ChunkTrigger.OnUnload))]
        public static bool OnUnload(ChunkTrigger __instance) {
            if (!FusionSceneManager.IsDelayedLoadDone())
                return true;

            if (NetworkInfo.HasServer) {
                var loader = SceneStreamer.Session.ChunkLoader;
                var chunk = __instance.chunk;
                bool canUnload = true;

                if (!TriggerUtilities.CanUnload(chunk)) {
                    canUnload = false;
                
                    if (loader._chunksToUnload.Has(chunk) && !loader._activeChunks.Has(chunk))
                    {
                        loader._chunksToUnload.Remove(chunk);
                        loader._activeChunks.Add(chunk);
                    }
                }

                return canUnload;
            }

            return true;
        }
    }
}

