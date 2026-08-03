using HarmonyLib;

using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Utilities;
using LabFusion.Entities;
using LabFusion.Player;

using UnityEngine;

using Avatar = Il2CppSLZ.VRMK.Avatar;

using Il2CppSLZ.Marrow;
using LabFusion.Marrow.Rig;

namespace LabFusion.Patching;

[HarmonyPatch(typeof(ArtRig))]
public static class ArtRigPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(nameof(ArtRig.ArtOutputUpdate))]
    public static void ArtOutputUpdate(ArtRig __instance, PhysicsRig inRig)
    {
        // Check if we have a player rep to animate the jaw on here
        if (!NetworkManager.HasServer)
        {
            return;
        }

        float angle = 0f;

        if (NetworkPlayerManager.TryGetPlayer(inRig.manager, out var player))
        {
            angle = player.JawFlapper.GetAngle();
        }

        var jaw = inRig.m_jaw;
        jaw.localRotation = Quaternion.AngleAxis(angle, Vector3Extensions.Right);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(ArtRig.ArtOutputLateUpdate))]
    public static void ArtOutputLateUpdate(ArtRig __instance, PhysicsRig inRig)
    {
        // Match the avatar jaw to the simulated jaw
        if (!NetworkManager.HasServer)
        {
            return;
        }

        var avatar = inRig.manager._avatar;

        var animatorJaw = avatar.animator.GetBoneTransform(HumanBodyBones.Jaw);

        if (animatorJaw != null)
        {
            animatorJaw.rotation = __instance.artJaw.rotation;
        }
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
            DelayUtilities.InvokeDelayed(() => { WaitForBarcode(inRig.manager, avatar); }, 2);
        }
        catch (Exception e)
        {
            FusionLogger.LogException("ArtRig.SetAvatar", e);
        }
    }

    private static void WaitForBarcode(RigManager rigManager, Avatar newAvatar)
    {
        // First make sure our player hasn't been destroyed (ex. loading new scene)
        if (rigManager == null)
        {
            return;
        }

        // Next check the avatar hasn't changed
        if (rigManager._avatar != newAvatar)
        {
            return;
        }

        if (NetworkBeingManager.TryGetNetworkRig(rigManager, out var networkRig))
        {
            networkRig.OnNewAvatarReady();
        }

        // Is this our local player? If so, sync the avatar change
        if (rigManager.IsLocalPlayer())
        {
            LocalAvatar.InvokeAvatarChanged(newAvatar, rigManager.AvatarCrate.Barcode.ID);
        }

        // If a NetworkPlayer is available, invoke it for that as well
        if (NetworkPlayerManager.TryGetPlayer(rigManager, out var networkPlayer))
        {
            NetworkAvatarManager.InvokeAvatarChanged(networkPlayer, newAvatar, rigManager.AvatarCrate.Barcode.ID);
        }
    }
}