using HarmonyLib;
using Il2CppSLZ.Marrow.Input;
using Il2CppSLZ.Rig;

using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(RemapRig))]
public static class RemapRigPatches
{
    private static bool LockedMovement(RemapRig __instance)
    {
        if (__instance.manager.IsSelf() && LocalControls.LockedMovement)
        {
            return true;
        }

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RemapRig.Jump))]
    public static bool Jump(RemapRig __instance)
    {
        // Lock jump charging
        if (LockedMovement(__instance))
        {
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RemapRig.JumpCharge))]
    public static bool JumpCharge(RemapRig __instance)
    {
        // Lock jump charging
        if (LockedMovement(__instance))
        {
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RemapRig.JumpEnd))]
    public static bool JumpEnd(RemapRig __instance)
    {
        // Lock jump charging
        if (LockedMovement(__instance))
        {
            return false;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(RemapRig.Jumping))]
    public static bool Jumping(RemapRig __instance)
    {
        // Lock jump charging
        if (LockedMovement(__instance))
        {
            return false;
        }

        return true;
    }
}