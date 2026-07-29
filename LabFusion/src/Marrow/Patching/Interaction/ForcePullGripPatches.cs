using HarmonyLib;

using Il2CppSLZ.Marrow;

using LabFusion.Entities;
using LabFusion.Scene;

namespace LabFusion.Marrow.Patching;

[HarmonyPatch(typeof(ForcePullGrip))]
public static class ForcePullGripPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ForcePullGrip.OnFarHandHoverUpdate))]
    public static bool OnFarHandHoverUpdatePrefix(ForcePullGrip __instance, ref bool __state, Hand hand)
    {
        __state = __instance.pullCoroutine != null;

        if (NetworkSceneManager.IsLevelNetworked && NetworkPlayerManager.HasExternalPlayer(hand.manager))
        {
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ForcePullGrip.OnFarHandHoverUpdate))]
    public static void OnFarHandHoverUpdatePostfix(ForcePullGrip __instance, ref bool __state, Hand hand)
    {
        if (!(__instance.pullCoroutine != null && !__state))
        {
            return;
        }

        var rigManager = hand.manager;

        if (NetworkRig.Cache.TryGet(rigManager, out var networkRig) && networkRig.NetworkEntity.IsOwner)
        {
            networkRig.RigGrabber.OnOwnedHandForcePull(hand, __instance._grip);
        }
    }
}
