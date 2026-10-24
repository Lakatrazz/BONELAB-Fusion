using HarmonyLib;

using Il2CppSLZ.Bonelab;

namespace MarrowFusion.Bonelab;

[HarmonyPatch(typeof(UI_ModGroup))]
public static class UI_ModGroupPatches
{
    // Temporary fix, 1.14 installs auto downloaded mods in your actual mods folder, so replacers still function
    // In 1.15 it should only prevent save deletion while in a server
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UI_ModGroup.BUTTON_DELETE))]
    public static bool BUTTON_DELETEPrefix() => false;
}
