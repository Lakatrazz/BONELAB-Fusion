using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Representation;
using LabFusion.Utilities;
using MelonLoader;
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
                var jaw = __instance.m_jaw;
                jaw.localRotation = Quaternion.AngleAxis(20f * rep.GetVoiceLoudness(), Vector3Extensions.right);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ArtRig.OnLateUpdate))]
        public static void OnLateUpdate(ArtRig __instance) {
            // If this is a player rep, match the avatar jaw to the simulated jaw
            if (NetworkInfo.HasServer && PlayerRepManager.HasPlayerId(__instance.manager))
            {
                var avatar = __instance.manager._avatar;

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

        [HarmonyPostfix]
        [HarmonyPatch(nameof(ArtRig.SetAvatar))]
        public static void SetAvatar(ArtRig __instance, Avatar avatar)
        {
            try
            {
                DelayUtilities.Delay(() => { Internal_WaitForBarcode(__instance.manager, avatar); }, 2);
            }
            catch (Exception e)
            {
                FusionLogger.LogException("ArtRig.SetAvatar", e);
            }
        }

        private static void Internal_WaitForBarcode(RigManager __instance, Avatar newAvatar)
        {
            // First make sure our player hasn't been destroyed (ex. loading new scene)
            if (__instance.IsNOC())
                return;

            // Next check the avatar hasn't changed
            if (__instance._avatar != newAvatar)
                return;

            // Is this our local player? If so, sync the avatar change
            if (__instance.IsLocalPlayer())
            {
                FusionPlayer.Internal_OnAvatarChanged(__instance, newAvatar, __instance.AvatarCrate.Barcode);
            }
            else if (PlayerRepManager.TryGetPlayerRep(__instance, out var rep))
            {
                rep.Internal_OnAvatarChanged(__instance.AvatarCrate.Barcode);
            }
        }
    }
}
