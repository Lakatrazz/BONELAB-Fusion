using UnityEngine;

using LabFusion.Utilities;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Entities;
using LabFusion.Marrow;

using Il2CppSLZ.Bonelab;

namespace LabFusion.Data;

public static class RigData
{
    public static RigRefs Refs { get; private set; } = new RigRefs();
    public static bool HasPlayer => Refs.IsValid;

    public static string RigAvatarId { get; internal set; } = MarrowBarcodes.EmptyBarcode;
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
        var playerRefs = PlayerRefs.Instance;

        if (playerRefs == null)
        {
            FusionLogger.Error("PlayerRefs does not exist, cannot get the player's RigManager!");
            return;
        }

        var manager = playerRefs.PlayerRigManager;

        if (manager == null)
        {
            FusionLogger.Error("Failed to get the player's RigManager!");
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
        var data = PlayerRepVitalsData.Create(PlayerIDManager.LocalSmallID, PlayerRefs.Instance.PlayerBodyVitals);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepVitals, CommonMessageRoutes.ReliableToOtherClients);
    }

    public static string GetAvatarBarcode()
    {
        var rm = Refs.RigManager;

        if (rm)
        {
            return rm.AvatarCrate.Barcode.ID;
        }

        return MarrowBarcodes.EmptyBarcode;
    }
}