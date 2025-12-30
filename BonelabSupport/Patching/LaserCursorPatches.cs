using HarmonyLib;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

using LabFusion.Scene;
using LabFusion.Utilities;

using UnityEngine;

namespace MarrowFusion.Bonelab.Patching;

[HarmonyPatch(typeof(LaserCursor))]
public static class LaserCursorPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(LaserCursor.Initialize))]
    public static void InitializePrefix(LaserCursor __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        ClearControllers(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(LaserCursor.Initialize))]
    public static void InitializePostfix(LaserCursor __instance)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return;
        }

        RemoveNetworkControllers(__instance);
    }

    private static void ClearControllers(LaserCursor laserCursor)
    {
        laserCursor.activeController = null;
        laserCursor.controllers = Array.Empty<BaseController>();
        laserCursor.controllerInput = new Il2CppSystem.Collections.Generic.Dictionary<BaseController, Transform>();
    }

    private static void RemoveNetworkControllers(LaserCursor laserCursor)
    {
        // Only add controllers from the local player
        // Otherwise, NetworkPlayers would be able to trigger the UI
        var controllers = new List<BaseController>();

        foreach (var controller in laserCursor.controllers)
        {
            if (controller.contRig.manager.IsLocalPlayer())
            {
                controllers.Add(controller);
            }
        }

        laserCursor.controllers = controllers.ToArray();

        // Also remove controllers from the controllerInput dictionary
        var controllerInput = new Il2CppSystem.Collections.Generic.Dictionary<BaseController, Transform>();

        foreach (var pair in laserCursor.controllerInput)
        {
            if (pair.Key.contRig.manager.IsLocalPlayer())
            {
                controllerInput.Add(pair.Key, pair.Value);
            }
        }

        laserCursor.controllerInput = controllerInput;
    }
}
