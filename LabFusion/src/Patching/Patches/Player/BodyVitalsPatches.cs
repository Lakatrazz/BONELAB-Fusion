using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Utilities;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(BodyVitals))]
public static class BodyVitalsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BodyVitals.ApplyUILayoutSettings))]
    public static bool ApplyUILayoutSettings(BodyVitals __instance)
    {
        var rig = __instance.ctrl_Rig;
        
        // Don't scale the UI rig if this isn't the main RigManager
        if (!rig.manager.IsSelf())
        {
            return false;
        }

        return true;
    }
}