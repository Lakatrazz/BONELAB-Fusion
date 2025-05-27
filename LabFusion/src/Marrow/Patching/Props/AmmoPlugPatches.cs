using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Patching;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(AmmoPlug))]
public static class AmmoPlugPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AmmoPlug.OnPlugInsertComplete))]
    public static void OnPlugInsertCompletePrefix()
    {
        PooleeDespawnPatch.IgnorePatch = true;
        AmmoSocketPatches.IgnorePatch = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(AmmoPlug.OnPlugInsertComplete))]
    public static void OnPlugInsertCompletePostfix()
    {
        PooleeDespawnPatch.IgnorePatch = false;
        AmmoSocketPatches.IgnorePatch = false;
    }
}