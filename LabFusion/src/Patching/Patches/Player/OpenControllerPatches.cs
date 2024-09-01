using HarmonyLib;

using UnityEngine;

using Il2CppSLZ.Marrow;

using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(OpenController))]
public static class OpenControllerPatches
{
    private static bool LockedMovement(OpenController __instance)
    {
        if (__instance.contRig.manager.IsSelf() && LocalControls.LockedMovement)
        {
            return true;
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(OpenController.OnUpdate))]
    public static void OnUpdate(OpenController __instance)
    {
        if (LockedMovement(__instance))
        {
            __instance._thumbstickAxis = Vector2.zero;
            __instance._aButton = false;
            __instance._aButtonUp = false;
        }
    }
}