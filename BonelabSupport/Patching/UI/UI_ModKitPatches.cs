using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Network;

namespace MarrowFusion.Bonelab;

[HarmonyPatch(typeof(UI_ModKit))]
public static class UI_ModKitPatches
{
    // Prevent mods from opening URLs while in a server
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UI_ModKit.OPENURL))]
    public static bool OPENURLPrefix() => !NetworkManager.HasServer;
}
