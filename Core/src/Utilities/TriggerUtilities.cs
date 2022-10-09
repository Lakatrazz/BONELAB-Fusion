using LabFusion.Extensions;
using SLZ.Zones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Utilities {
    public static class TriggerUtilities {
        public static Dictionary<SceneZone, int> zoneCount = new Dictionary<SceneZone, int>(new UnityComparer());
        public static Dictionary<TriggerLasers, int> triggerCount = new Dictionary<TriggerLasers, int>(new UnityComparer());

        public static void Increment(SceneZone zone) {
            if (!zoneCount.ContainsKey(zone))
                zoneCount.Add(zone, 0);

            zoneCount[zone]++;
        }

        public static void Decrement(SceneZone zone)
        {
            if (!zoneCount.ContainsKey(zone))
                zoneCount.Add(zone, 0);

            zoneCount[zone]--;
            zoneCount[zone] = Mathf.Clamp(zoneCount[zone], 0, int.MaxValue);
        }

        public static bool CanEnter(SceneZone zone)
        {
            if (!zoneCount.ContainsKey(zone))
                return false;

            return zoneCount[zone] <= 1;
        }

        public static bool CanExit(SceneZone zone)
        {
            if (!zoneCount.ContainsKey(zone))
                return false;

            return zoneCount[zone] <= 0;
        }

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
    }

}
