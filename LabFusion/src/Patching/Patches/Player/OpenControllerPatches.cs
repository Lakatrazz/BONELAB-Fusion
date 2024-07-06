﻿using HarmonyLib;
using Il2CppSLZ.Rig;

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
    [HarmonyPatch(nameof(OpenController.GetThumbStickAxis))]
    public static bool GetThumbstickAxis(OpenController __instance)
    {
        // Lock movement
        if (LockedMovement(__instance))
        {
            return false;
        }

        return true;
    }
}