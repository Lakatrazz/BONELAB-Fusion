using Il2CppSLZ.Interaction;

using HarmonyLib;

using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Network;
using LabFusion.Entities;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(BarrelGrip))]
    public static class BarrelGripPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BarrelGrip.UpdateJointConfiguration))]
        public static void UpdateJointConfiguration(Hand hand) => GripPatches.UpdateJointConfiguration(hand);
    }

    [HarmonyPatch(typeof(SphereGrip))]
    public static class SphereGripPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(SphereGrip.UpdateJointConfiguration))]
        public static void UpdateJointConfiguration(Hand hand) => GripPatches.UpdateJointConfiguration(hand);
    }

    [HarmonyPatch(typeof(GenericGrip))]
    public static class GenericGripPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(GenericGrip.UpdateJointConfiguration))]
        public static void UpdateJointConfiguration(Hand hand) => GripPatches.UpdateJointConfiguration(hand);
    }

    [HarmonyPatch(typeof(TargetGrip))]
    public static class TargetGripPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(TargetGrip.UpdateJointConfiguration))]
        public static void UpdateJointConfiguration(Hand hand) => GripPatches.UpdateJointConfiguration(hand);
    }

    [HarmonyPatch(typeof(CylinderGrip))]
    public static class CylinderGripPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(CylinderGrip.UpdateJointConfiguration))]
        public static void UpdateJointConfiguration(Hand hand) => GripPatches.UpdateJointConfiguration(hand);
    }

    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverUpdate))]
    public static class ForcePullPatches
    {
        public static bool Prefix(ForcePullGrip __instance, ref bool __state, Hand hand)
        {
            __state = __instance.pullCoroutine != null;

            if (NetworkInfo.HasServer && NetworkPlayerManager.HasExternalPlayer(hand.manager))
            {
                return false;
            }

            return true;
        }

        public static void Postfix(ForcePullGrip __instance, ref bool __state, Hand hand)
        {
            if (!(__instance.pullCoroutine != null && !__state))
                return;

            GrabHelper.SendObjectForcePull(hand, __instance._grip);
        }
    }

    [HarmonyPatch(typeof(Grip))]
    public static class GripPatches
    {
        // This is just referenced by other grip patches, not actually a patch itself
        public static void UpdateJointConfiguration(Hand hand)
        {
            if (NetworkInfo.HasServer && NetworkPlayerManager.HasExternalPlayer(hand.manager))
            {
                var joint = hand.joint;

                joint.breakForce = float.PositiveInfinity;
                joint.breakTorque = float.PositiveInfinity;
            }
        }

        [HarmonyPatch(nameof(Grip.OnAttachedToHand))]
        [HarmonyPostfix]
        private static void OnAttachedToHand(Grip __instance, Hand hand)
        {
            if (hand.manager.IsSelf())
            {
                GrabHelper.SendObjectAttach(hand, __instance);
            }
        }

        [HarmonyPatch(nameof(Grip.OnDetachedFromHand))]
        [HarmonyPostfix]
        private static void OnDetachedFromHand(Hand hand)
        {
            if (hand.manager.IsSelf())
            {
                GrabHelper.SendObjectDetach(hand);
            }
        }
    }
}
