using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Scene;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(AmmoPlug))]
public static class AmmoPlugPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AmmoPlug.OnPlugInsertComplete))]
    public static void OnPlugInsertCompletePrefix(AmmoPlug __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        var socket = __instance._lastSocket;

        if (socket != null && socket.IsClearOnInsert)
        {
            PooleeDespawnPatch.IgnorePatch = true;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(AmmoPlug.OnPlugInsertComplete))]
    public static void OnPlugInsertCompletePostfix(AmmoPlug __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        PooleeDespawnPatch.IgnorePatch = false;
    }
}