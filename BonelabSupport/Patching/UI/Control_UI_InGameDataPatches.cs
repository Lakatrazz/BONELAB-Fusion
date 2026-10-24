using HarmonyLib;

using Il2CppSLZ.Bonelab;

using LabFusion.Network;

namespace MarrowFusion.Bonelab;

[HarmonyPatch(typeof(Control_UI_InGameData))]
public static class Control_UI_InGameDataPatches
{
    // Prevent mods from deleting save files while in a server
    [HarmonyPatch(nameof(Control_UI_InGameData.DATA_DELETE))]
    public static bool DATA_DELETEPrefix() => !NetworkManager.HasServer;

    [HarmonyPatch(nameof(Control_UI_InGameData.DATA_DELETE_ALL))]
    public static bool DATA_DELETEALLPrefix() => !NetworkManager.HasServer;
}
