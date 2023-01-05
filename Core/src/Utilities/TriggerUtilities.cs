using LabFusion.Data;
using LabFusion.Extensions;

using SLZ.AI;
using SLZ.Rig;
using SLZ.Zones;
using SLZ.Bonelab;

using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using LabFusion.Network;
using SLZ.Marrow.SceneStreaming;
using MelonLoader;

namespace LabFusion.Utilities {
    public static class TriggerUtilities {
        public static readonly Dictionary<TriggerLasers, int> TriggerCount = new Dictionary<TriggerLasers, int>(new UnityComparer());
        public static readonly Dictionary<Chunk, int> ChunkCount = new Dictionary<Chunk, int>(new UnityComparer());

        internal static void Increment(TriggerLasers trigger) {
            if (!TriggerCount.ContainsKey(trigger))
                TriggerCount.Add(trigger, 0);

            TriggerCount[trigger]++;
        }

        internal static void Decrement(TriggerLasers trigger) {
            if (!TriggerCount.ContainsKey(trigger))
                TriggerCount.Add(trigger, 0);

            TriggerCount[trigger]--;
            TriggerCount[trigger] = Mathf.Clamp(TriggerCount[trigger], 0, int.MaxValue);
        }

        internal static void Increment(Chunk chunk) {
            var chunks = chunk.GetChunks();

            foreach (var found in chunks) {
                if (!ChunkCount.ContainsKey(found))
                    ChunkCount.Add(found, 0);

                ChunkCount[found]++;
            }
        }

        internal static void Decrement(Chunk chunk) {
            MelonCoroutines.Start(CoDelayedDecrement(chunk));
        }

        private static IEnumerator CoDelayedDecrement(Chunk chunk) {
            // Delay a while
            for (var i = 0; i < 300; i++) {
                yield return null;
            }

            // Decrement chunks
            var chunks = chunk.GetChunks();

            foreach (var found in chunks) {
                if (!ChunkCount.ContainsKey(found))
                    ChunkCount.Add(found, 0);

                ChunkCount[found]--;
                ChunkCount[found] = Mathf.Clamp(ChunkCount[found], 0, int.MaxValue);
            }
        }

        internal static bool CanUnload(Chunk chunk) {
            if (!ChunkCount.ContainsKey(chunk))
                return false;

            return ChunkCount[chunk] <= 0;
        }

        public static bool CanEnter(TriggerLasers trigger)
        {
            if (!TriggerCount.ContainsKey(trigger))
                return false;

            return TriggerCount[trigger] <= 1;
        }

        public static bool CanExit(TriggerLasers trigger)
        {
            if (!TriggerCount.ContainsKey(trigger))
                return false;

            return TriggerCount[trigger] <= 0;
        }

        public static bool IsMainRig(Collider other) {
            if (!NetworkInfo.HasServer || RigData.RigReferences.RigManager.IsNOC())
                return true;

            var trigger = TriggerRefProxy.Cache.Get(other.gameObject);
            RigManager rig;

            if (trigger && trigger.root && (rig = RigManager.Cache.Get(trigger.root))) {
                return rig == RigData.RigReferences.RigManager;
            }

            return false;
        }
    }
}
