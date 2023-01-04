using LabFusion.Data;
using LabFusion.Extensions;

using SLZ.AI;
using SLZ.Rig;
using SLZ.Zones;
using SLZ.Bonelab;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using LabFusion.Network;
using SLZ.Marrow.SceneStreaming;

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
            if (!ChunkCount.ContainsKey(chunk))
                ChunkCount.Add(chunk, 0);

            ChunkCount[chunk]++;
        }

        internal static void Decrement(Chunk chunk) {
            if (!ChunkCount.ContainsKey(chunk))
                ChunkCount.Add(chunk, 0);

            ChunkCount[chunk]--;
            ChunkCount[chunk] = Mathf.Clamp(ChunkCount[chunk], 0, int.MaxValue);
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
