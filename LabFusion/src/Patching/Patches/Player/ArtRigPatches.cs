using HarmonyLib;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Entities;

using Il2CppSLZ.Rig;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(ArtRig))]
public static class ArtRigPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ArtRig.ArtOutputUpdate))]
    public static void ArtOutputUpdate(ArtRig __instance, PhysicsRig inRig)
    {
        // Check if we have a player rep to animate the jaw on here
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        float angle = 0f;

        if (NetworkPlayerManager.TryGetPlayer(inRig.manager, out var player))
        {
            angle = player.JawFlapper.GetAngle();
        }

        var jaw = inRig.m_jaw;
        jaw.localRotation = Quaternion.AngleAxis(angle, Vector3Extensions.right);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ArtRig.ArtOutputLateUpdate))]
    public static void ArtOutputLateUpdate(ArtRig __instance, PhysicsRig inRig)
    {
        // Match the avatar jaw to the simulated jaw
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var avatar = inRig.manager._avatar;

        var animatorJaw = avatar.animator.GetBoneTransform(HumanBodyBones.Jaw);

        if (animatorJaw != null)
            animatorJaw.rotation = __instance.artJaw.rotation;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ArtRig.ApplyRotationOffsetsToRig))]
    public static void ApplyRotationOffsetsToRig(ArtRig __instance, Avatar avatar)
    {
        // The game doesn't setup the jaw by default
        var artJaw = __instance.artJaw;
        artJaw.localRotation = avatar.artOffsets.jawOffset;
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ArtRig.SetArtOutputAvatar))]
    public static void SetArtOutputAvatar(ArtRig __instance, PhysicsRig inRig, Avatar avatar)
    {
        try
        {
            DelayUtilities.Delay(() => { Internal_WaitForBarcode(inRig.manager, avatar); }, 2);
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
        if (__instance.IsSelf())
        {
            FusionPlayer.Internal_OnAvatarChanged(__instance, newAvatar, __instance.AvatarCrate.Barcode);
        }
        else if (NetworkPlayerManager.TryGetPlayer(__instance, out var player))
        {
            player.Internal_OnAvatarChanged(__instance.AvatarCrate.Barcode);
        }
    }
}