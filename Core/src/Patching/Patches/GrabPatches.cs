using SLZ.Interaction;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using HarmonyLib;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;
using LabFusion.Grabbables;
using LabFusion.Network;
using LabFusion.Extensions;

namespace LabFusion.Patching
{
    [HarmonyPatch(typeof(ForcePullGrip), nameof(ForcePullGrip.OnFarHandHoverUpdate))]
    public class ForcePullPatch
    {
        public static bool Prefix(ForcePullGrip __instance, ref bool __state, Hand hand)
        {
            __state = __instance.pullCoroutine != null;

            if (NetworkInfo.HasServer && PlayerRepManager.HasPlayerId(hand.manager)) {
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

    [HarmonyPatch(typeof(InteractableHost))]
    public static class InteractableHostPatches {
        public static bool IgnorePatches = false;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(InteractableHost.ForceDetach))]
        public static bool ForceDetach(InteractableHost __instance) {
            if (IgnorePatches)
                return true;

            if (NetworkInfo.HasServer) {
                foreach (var hand in __instance._hands) {
                    if (PlayerRepManager.HasPlayerId(hand.manager))
                        return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Grip))]
    public static class GripPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Grip.OnAttachedToHand))]
        public static void OnAttachedToHand(Grip __instance, Hand hand)
        {
            try
            {
                // Make sure this is the main rig
                if (hand.manager != RigData.RigReferences.RigManager)
                    return;

                GrabHelper.SendObjectAttach(hand, __instance);
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch Grip.OnAttachedToHand", e);
#endif
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Grip.OnDetachedFromHand))]
        public static void OnDetachedFromHand(Grip __instance, Hand hand)
        {
            try
            {
                // Make sure this is the main rig
                if (hand.manager != RigData.RigReferences.RigManager)
                    return;

                GrabHelper.SendObjectDetach(hand);
            }
            catch (Exception e)
            {
#if DEBUG
                FusionLogger.LogException("to execute patch Hand.DetachObject", e);
#endif
            }
        }
    }
}
