using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Scene;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(VirtualController))]
public static class VirtualControllerPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(VirtualController.CheckHandDesync))]
    public static bool CheckHandDesyncPrefix()
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        return false;
    }
}
