using LabFusion.Data;
using LabFusion.Grabbables;
using LabFusion.Utilities;
using MelonLoader;
using SLZ.Interaction;
using System;
using System.Linq;
using System.Reflection;

namespace LabFusion.Patching
{
    internal class ManualPatchRunner
    {
        public static void Init(HarmonyLib.Harmony HarmonyInstance)
        {
            // Please do not ask why it's done like this. I have no answers
            var patches = typeof(Grip).GetMethods().Where(a => a.Name == nameof(Grip.OnAttachedToHand)).Cast<MethodBase>();
            foreach (var patch in patches)
            {
                HarmonyInstance.Patch(patch, typeof(GripPatches).GetMethod(nameof(GripPatches.OnAttachedToHand)).ToNewHarmonyMethod());
            }

            patches = typeof(Hand).GetMethods().Where(a => a.Name == nameof(Hand.DetachJoint)).Cast<MethodBase>();
            foreach (var patch in patches)
            {
                HarmonyInstance.Patch(patch, typeof(ManualPatchRunner).GetMethod(nameof(OnDetachedFromHand)).ToNewHarmonyMethod());
                break;
            }
        }

        public static void OnDetachedFromHand(Hand __instance)
        {
            Hand hand = __instance;
            if (hand.AttachedReceiver == null)
                return;

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
