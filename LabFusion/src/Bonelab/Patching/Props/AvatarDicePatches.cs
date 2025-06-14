using HarmonyLib;

using LabFusion.Network;

using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow;

namespace LabFusion.Bonelab.Patching;

[HarmonyPatch(typeof(AvatarDice))]
public static class AvatarDicePatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AvatarDice.OnHandAttached))]
    public static void OnHandAttached(AvatarDice __instance, InteractableHost host, Hand hand)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Update the manager to the person grabbing the dice
        __instance.rigManager = hand.manager;
    }
}
