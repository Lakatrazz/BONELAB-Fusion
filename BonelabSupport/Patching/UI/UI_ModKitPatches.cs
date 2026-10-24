using HarmonyLib;

using Il2CppSLZ.Bonelab;

namespace MarrowFusion.Bonelab;

[HarmonyPatch(typeof(UI_ModKit))]
public static class UI_ModKitPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UI_ModKit.OPENURL))]
    public static bool OPENURLPrefix() => false;
}
