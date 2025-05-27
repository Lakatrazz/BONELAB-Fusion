using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Scene;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(AlignPlug))]
public static class AlignPlugPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(AlignPlug.OnHandAttached))]
    public static bool OnHandAttached(AlignPlug __instance, InteractableHost host, Hand hand) => ValidatePlug(__instance, hand);

    [HarmonyPrefix]
    [HarmonyPatch(nameof(AlignPlug.OnHandDetached))]
    public static bool OnHandDetached(AlignPlug __instance, InteractableHost host, Hand hand) => ValidatePlug(__instance, hand);

    [HarmonyPatch(nameof(AlignPlug.OnProxyGrab))]
    [HarmonyPrefix]
    public static bool OnProxyGrab(AlignPlug __instance, Hand hand) => ValidatePlug(__instance, hand);

    [HarmonyPatch(nameof(AlignPlug.OnProxyRelease))]
    [HarmonyPrefix]
    public static bool OnProxyRelease(AlignPlug __instance, Hand hand) => ValidatePlug(__instance, hand);

    private static bool ValidatePlug(AlignPlug plug, Hand hand)
    {
        if (!NetworkSceneManager.IsLevelNetworked)
        {
            return true;
        }

        if (plug.TryCast<AmmoPlug>() == null)
        {
            return true;
        }

        if (NetworkPlayerManager.HasExternalPlayer(hand.manager))
        {
            return false;
        }

        return true;
    }
}