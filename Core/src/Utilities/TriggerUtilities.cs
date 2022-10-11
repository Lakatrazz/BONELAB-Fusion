using LabFusion.Data;
using LabFusion.Extensions;
using SLZ.AI;
using SLZ.Rig;
using SLZ.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Utilities {
    public static class TriggerUtilities {
        public static Dictionary<TriggerLasers, int> triggerCount = new Dictionary<TriggerLasers, int>(new UnityComparer());

        public static void Increment(TriggerLasers trigger)
        {
            if (!triggerCount.ContainsKey(trigger))
                triggerCount.Add(trigger, 0);

            triggerCount[trigger]++;
        }

        public static void Decrement(TriggerLasers trigger)
        {
            if (!triggerCount.ContainsKey(trigger))
                triggerCount.Add(trigger, 0);

            triggerCount[trigger]--;
            triggerCount[trigger] = Mathf.Clamp(triggerCount[trigger], 0, int.MaxValue);
        }

        public static bool CanEnter(TriggerLasers trigger)
        {
            if (!triggerCount.ContainsKey(trigger))
                return false;

            return triggerCount[trigger] <= 1;
        }

        public static bool CanExit(TriggerLasers trigger)
        {
            if (!triggerCount.ContainsKey(trigger))
                return false;

            return triggerCount[trigger] <= 0;
        }


        public static bool IsMainRig(Collider other) {
            var trigger = TriggerRefProxy.Cache.Get(other.gameObject);
            RigManager rig;

            if (trigger && trigger.root && (rig = RigManager.Cache.Get(trigger.root))) {
                return rig == RigData.RigManager;
            }

            return false;
        }
    }
}
