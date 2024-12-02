using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Entities;

using Il2CppSLZ.Bonelab;

using CommonBarcodes = LabFusion.Utilities.CommonBarcodes;

namespace LabFusion.Data;

public static class RigData
{
    public static RigRefs Refs { get; private set; } = new RigRefs();
    public static bool HasPlayer => Refs.IsValid;

    public static string RigAvatarId { get; internal set; } = CommonBarcodes.INVALID_AVATAR_BARCODE;
    public static SerializedAvatarStats RigAvatarStats { get; internal set; } = null;

    public static Vector3 RigSpawn { get; private set; }
    public static Quaternion RigSpawnRot { get; private set; }

    private static void OnJump()
    {
        if (NetworkInfo.HasServer)
        {
            PlayerSender.SendPlayerAction(PlayerActionType.JUMP);
        }
    }

    public static void OnCacheRigInfo()
    {
        var manager = PlayerRefs.Instance.PlayerRigManager;

        if (manager == null)
        {
            FusionLogger.Error("Failed to find the Player's RigManager!");
            return;
        }

        // Store spawn values
        RigSpawn = manager.transform.position;
        RigSpawnRot = manager.transform.rotation;

        // Store the references
        Refs = new RigRefs(manager);
        Refs.RigManager.remapHeptaRig.onPlayerJump += (Il2CppSystem.Action)OnJump;

        PlayerRefs.Instance.PlayerBodyVitals.rescaleEvent += (BodyVitals.RescaleUI)OnSendVitals;

        // Notify hooks
        LocalPlayer.OnLocalRigCreated?.InvokeSafe(manager, "executing OnLocalRigCreated hook");

        // Update avatar
        if (manager._avatar != null)
        {
            LocalAvatar.InvokeAvatarChanged(manager._avatar, manager.AvatarCrate.Barcode.ID);
        }
    }

    public static void OnSendVitals()
    {
        // Make sure we have a server
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        // Send body vitals to network
        using FusionWriter writer = FusionWriter.Create(PlayerRepVitalsData.Size);
        var data = PlayerRepVitalsData.Create(PlayerIdManager.LocalSmallId, PlayerRefs.Instance.PlayerBodyVitals);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.PlayerRepVitals, writer);
        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
    }

    public static string GetAvatarBarcode()
    {
        var rm = Refs.RigManager;

        if (rm)
        {
            return rm.AvatarCrate.Barcode.ID;
        }

        return CommonBarcodes.INVALID_AVATAR_BARCODE;
    }
}