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

namespace LabFusion.Utilities {
    public static class TriggerUtilities {
        public static readonly Dictionary<TriggerLasers, int> TriggerCount = new Dictionary<TriggerLasers, int>(new UnityComparer());

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
            var trigger = TriggerRefProxy.Cache.Get(other.gameObject);
            RigManager rig;

            if (trigger && trigger.root && (rig = RigManager.Cache.Get(trigger.root))) {
                return rig == RigData.RigReferences.RigManager;
            }

            return false;
        }
    }
}
