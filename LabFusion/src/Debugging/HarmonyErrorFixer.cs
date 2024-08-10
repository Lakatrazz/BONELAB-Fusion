#if DEBUG
using System.Reflection;

using HarmonyLib;

using MelonLoader;

namespace LabFusion.Debugging;

[HarmonyPatch]
public static class Il2CppDetourMethodPatcherPatches
{
    public static MethodBase TargetMethod()
    {
        var type = Type.GetType("Il2CppInterop.HarmonySupport.Il2CppDetourMethodPatcher, Il2CppInterop.HarmonySupport", true);
        var method = AccessTools.FirstMethod(type, (method) => { return method.Name.Contains("ReportException"); });
        return method;
    }

    public static bool Prefix(Exception ex)
    {
        MelonLogger.Error("During invoking native->managed trampoline", ex);
        return false;
    }
}
#endif