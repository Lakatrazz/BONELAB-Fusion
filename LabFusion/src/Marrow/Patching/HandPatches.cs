using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(Hand))]
public static class HandPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(Hand.UpdateHovering))]
    public static bool UpdateHoveringPrefix(Hand __instance)
    {
        if (!__instance.manager.IsLocalPlayer())
        {
            return true;
        }

        if (LocalControls.DisableInteraction)
        {
            __instance.HoveringReceiver = null;
            __instance.farHoveringReciever = null;

            return false;
        }

        return true;
    }
}
