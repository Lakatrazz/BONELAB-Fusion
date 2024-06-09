using HarmonyLib;

using LabFusion.Network;

using Il2CppSLZ.Interaction;
using Il2CppSLZ.Bonelab;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(AvatarDice))]
    public static class AvatarDicePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(AvatarDice.OnHandAttached))]
        public static void OnHandAttached(AvatarDice __instance, InteractableHost host, Hand hand)
        {
            // Force update the manager
            if (NetworkInfo.HasServer)
            {
                __instance.rigManager = hand.manager;
            }
        }
    }
}
