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
        public static readonly Dictionary<GenGameControl_Trigger, int> GenTriggerCount = new Dictionary<GenGameControl_Trigger, int>(new UnityComparer());
        public static readonly Dictionary<TriggerRefProxy, List<Chunk>> PlayerChunks = new Dictionary<TriggerRefProxy, List<Chunk>>(new UnityComparer());

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

        internal static void Increment(GenGameControl_Trigger trigger)
        {
            if (!GenTriggerCount.ContainsKey(trigger))
                GenTriggerCount.Add(trigger, 0);

            GenTriggerCount[trigger]++;
        }

        internal static void Decrement(GenGameControl_Trigger trigger)
        {
            if (!GenTriggerCount.ContainsKey(trigger))
                GenTriggerCount.Add(trigger, 0);

            GenTriggerCount[trigger]--;
            GenTriggerCount[trigger] = Mathf.Clamp(GenTriggerCount[trigger], 0, int.MaxValue);
        }

        internal static void SetChunk(Collider other, Chunk chunk) {
            var proxy = TriggerRefProxy.Cache.Get(other.gameObject);
            if (!proxy)
                return;

            if (!PlayerChunks.ContainsKey(proxy)) {
                PlayerChunks.Add(proxy, new List<Chunk>());
            }

            PlayerChunks[proxy] = chunk.GetChunks();
        }

        internal static bool CanUnload(Chunk chunk) {
            foreach (var pair in PlayerChunks) {
                if (pair.Value.Has(chunk))
                    return false;
            }

            return true;
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

        public static bool CanEnter(GenGameControl_Trigger trigger)
        {
            if (!GenTriggerCount.ContainsKey(trigger))
                return false;

            return GenTriggerCount[trigger] <= 1;
        }

        public static bool CanExit(GenGameControl_Trigger trigger)
        {
            if (!GenTriggerCount.ContainsKey(trigger))
                return false;

            return GenTriggerCount[trigger] <= 0;
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
