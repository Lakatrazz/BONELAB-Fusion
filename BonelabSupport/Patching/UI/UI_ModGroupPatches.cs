using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Network;

namespace MarrowFusion.Bonelab;

[HarmonyPatch(typeof(UI_ModGroup))]
public static class UI_ModGroupPatches
{
    // Prevent mods from deleting save files while in a server
    [HarmonyPrefix]
    [HarmonyPatch(nameof(UI_ModGroup.BUTTON_DELETE))]
    public static bool BUTTON_DELETEPrefix() => !NetworkManager.HasServer;
}
