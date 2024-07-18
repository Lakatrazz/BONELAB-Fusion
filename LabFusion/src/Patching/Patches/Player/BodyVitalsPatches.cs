using HarmonyLib;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

using LabFusion.Utilities;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(BodyVitals))]
public static class BodyVitalsPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(BodyVitals.ApplyUILayoutSettings))]
    public static bool ApplyUILayoutSettings(BodyVitals __instance)
    {
        var rig = __instance.GetComponent<RigManager>();
        
        // Don't scale the UI rig if this isn't the main RigManager
        if (!rig.IsSelf())
        {
            return false;
        }

        return true;
    }
}