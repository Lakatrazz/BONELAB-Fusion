using LabFusion.Network;
using LabFusion.Representation;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Marrow.Utilities;

using HarmonyLib;
using LabFusion.Entities;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(VirtualController))]
    public static class VirtualControllerPatches
    {
        [HarmonyPatch(nameof(VirtualController.CheckHandDesync))]
        [HarmonyPrefix]
        public static bool CheckHandDesync(HandGripPair pair, SimpleTransform contHandle, SimpleTransform rigHandle)
        {
            if (NetworkInfo.HasServer)
            {
                var hand = pair.hand;

                if (hand != null && NetworkPlayerManager.HasExternalPlayer(hand.manager))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
