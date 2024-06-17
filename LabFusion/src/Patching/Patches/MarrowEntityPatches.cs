using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Il2CppSLZ.Marrow.Interaction;
using Il2CppSLZ.Marrow.Zones;

using LabFusion.Entities;
using LabFusion.Network;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(MarrowEntity))]
public static class MarrowEntityPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(MarrowEntity.OnCullApply))]
    public static void OnCullApply(MarrowEntity __instance, InactiveStatus status, bool isInactive)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var entity = IMarrowEntityExtender.Cache.Get(__instance);

        if (entity == null)
        {
            return;
        }

        var extender = entity.GetExtender<IMarrowEntityExtender>();

        extender.OnEntityCull(isInactive);
    }
}
