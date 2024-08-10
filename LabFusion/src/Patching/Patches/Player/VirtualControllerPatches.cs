using LabFusion.Network;
using LabFusion.Entities;

using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Utilities;

using HarmonyLib;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(VirtualController))]
public static class VirtualControllerPatches
{
    [HarmonyPatch(nameof(VirtualController.CheckHandDesync))]
    [HarmonyPrefix]
    public static bool CheckHandDesync(HandGripPair pair, SimpleTransform contHandle, SimpleTransform rigHandle)
    {
        if (!NetworkInfo.HasServer)
        {
            return true;
        }

        // The hand and its rigManager will never be null, so we don't need to check for it
        // If they are null, then something has seriously gone wrong, so an error *should* be thrown and not hidden
        var hand = pair.hand;

        if (NetworkPlayerManager.HasExternalPlayer(hand.manager))
        {
            return false;
        }

        return true;
    }
}