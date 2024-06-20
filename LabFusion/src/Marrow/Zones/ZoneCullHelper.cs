using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Zones;

namespace LabFusion.Marrow.Zones;

public static class ZoneCullHelper
{
    public static void MigrateEntity(int cullerId, MarrowEntity entity)
    {
        var cullManager = ZoneManagerPlugin.ZoneCullManager;
        var inactiveStatus = entity._inactiveStatus;

        // Unregister entity from existing cullers
        if (cullManager._entityToCullerId.ContainsKey(inactiveStatus))
        {
            var cullerIds = cullManager._entityToCullerId[inactiveStatus].ToArray();

            foreach (var id in cullerIds)
            {
                cullManager.Unregister(id, inactiveStatus);
            }
        }

        // Register entity in the new culler
        cullManager.Register(cullerId, entity);

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