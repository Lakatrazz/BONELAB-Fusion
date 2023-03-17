using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;

using SLZ.Rig;

using UnityEngine;

using Avatar = SLZ.VRMK.Avatar;

namespace LabFusion.Patching {
    [HarmonyPatch(typeof(ArtRig))]
    public static class ArtRigPatches {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ArtRig.OnUpdate))]
        public static void OnUpdate(ArtRig __instance) {
            // Check if we have a player rep to animate the jaw on here
            if (NetworkInfo.HasServer && PlayerRepManager.TryGetPlayerRep(__instance.manager, out var rep)) {
                var avatar = __instance.manager._avatar;

                if (!avatar.gameObject.activeInHierarchy) {
                    return;
                }

                var jaw = __instance.m_jaw;
                jaw.localRotation = Quaternion.AngleAxis(20f * rep.GetVoiceLoudness(), Vector3Extensions.right);

                var animatorJaw = avatar.animator.GetBoneTransform(HumanBodyBones.Jaw);

                if (animatorJaw != null)
                    animatorJaw.rotation = __instance.artJaw.rotation;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ArtRig.ApplyRotationOffsetsToRig))]
        public static void ApplyRotationOffsetsToRig(ArtRig __instance, Avatar avatar) {
            // The game doesn't setup the jaw by default
            var artJaw = __instance.artJaw;
            artJaw.localRotation = avatar.artOffsets.jawOffset;
        }
    }
}
