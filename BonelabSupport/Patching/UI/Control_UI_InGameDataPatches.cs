using HarmonyLib;

using Il2CppSLZ.Bonelab;

namespace MarrowFusion.Bonelab;

[HarmonyPatch(typeof(Control_UI_InGameData))]
public static class Control_UI_InGameDataPatches
{
    // Temporary fix, 1.14 installs auto downloaded mods in your actual mods folder, so replacers still function
    // In 1.15 it should only prevent save deletion while in a server
    [HarmonyPatch(nameof(Control_UI_InGameData.DATA_DELETE))]
    public static bool DATA_DELETEPrefix() => false;

    [HarmonyPatch(nameof(Control_UI_InGameData.DATA_DELETE_ALL))]
    public static bool DATA_DELETEALLPrefix() => false;
}
