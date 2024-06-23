using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Zones;

using LabFusion.Patching;
using LabFusion.Utilities;

namespace LabFusion.Marrow.Zones;

public static class ZoneCullHelper
{
    private static void SimulateZoneExit(Zone zone, MarrowEntity entity) 
    {
        // Exit all trackers
        foreach (var body in entity.Bodies)
        {
            foreach (var tracker in body.Trackers)
            {
                // Make sure the zone still contains the entity
                if (!zone._entityOverlapCounts.ContainsKey(entity))
                {
                    return;
                }

                // Make sure the zone contains the tracker
                if (!zone._trackerOverlap.Contains(tracker))
                {
                    continue;
                }

                try
                {
                    zone.OnTriggerExit(tracker.Collider);
                }
                catch (Exception e)
                {
#if DEBUG
                    FusionLogger.LogException("simulating zone exit", e);
#endif
                }

                zone._trackerOverlap.Remove(tracker);
            }
        }

        zone._entityOverlapCounts.Remove(entity);
    }

    private static void SimulateZoneEnter(Zone zone, MarrowEntity entity) 
    {
        foreach (var body in entity.Bodies)
        {
            foreach (var tracker in body.Trackers)
            {
                if (zone._trackerOverlap.Contains(tracker))
                {
                    continue;
                }

                try
                {
                    zone.OnTriggerEnter(tracker.Collider);
                }
                catch (Exception e)
                {
#if DEBUG
                    FusionLogger.LogException("simulating zone enter", e);
#endif
                }
            }
        }

        // Make sure the zone's overlap count is only 1
        if (!zone._entityOverlapCounts.ContainsKey(entity))
        {
            zone._entityOverlapCounts.Add(entity, 1);
        }
        else
        {
            zone._entityOverlapCounts[entity] = 1;
        }
    }

    public static void MigrateEntity(int cullerId, MarrowEntity entity)
    {
        // Make sure the culler can be found
        if (!ZoneCullerPatches.CullerIdToZone.TryGetValue(cullerId, out var cullerToRegister))
        {
            return;
        }

        var cullManager = ZoneManagerPlugin.ZoneCullManager;
        var inactiveStatus = entity._inactiveStatus;

        // Exit entity from existing zones
        if (cullManager._entityToCullerId.ContainsKey(inactiveStatus))
        {
            var cullerIds = cullManager._entityToCullerId[inactiveStatus].ToArray();

            foreach (var id in cullerIds)
            {
                if (ZoneCullerPatches.CullerIdToZone.TryGetValue(id, out var cullerToUnregister))
                {
                    SimulateZoneExit(cullerToUnregister._zone, entity);
                }
            }
        }

        // Enter entity into new zone
        SimulateZoneEnter(cullerToRegister._zone, entity);

        // Update cull state if needed
        cullManager.SolveCullState(inactiveStatus);
    }

    public static int? GetLastCullerId(MarrowEntity entity)
    {
        var cullManager = ZoneManagerPlugin.ZoneCullManager;
        var inactiveStatus = entity._inactiveStatus;

        if (cullManager._orphanEntityToLastCullerId.ContainsKey(inactiveStatus))
        {
            return cullManager._orphanEntityToLastCullerId[inactiveStatus];
        }

        if (cullManager._entityToCullerId.ContainsKey(inactiveStatus))
        {
            var list = cullManager._entityToCullerId[inactiveStatus];

            if (list.Count > 0)
            {
                return list[0];
            }
        }

        return null;
    }
}