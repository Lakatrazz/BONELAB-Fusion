using HarmonyLib;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

using LabFusion.Scene;
using LabFusion.Utilities;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(LaserCursor))]
public static class LaserCursorPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(LaserCursor.Initialize))]
    public static void Initialize(LaserCursor __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        // Only add controllers from the local player
        // Otherwise, NetworkPlayers would be able to trigger the UI
        var controllers = new List<BaseController>();

        foreach (var controller in __instance.controllers)
        {
            if (controller.contRig.manager.IsLocalPlayer())
            {
                controllers.Add(controller);
            }
        }

        __instance.controllers = controllers.ToArray();
    }
}
