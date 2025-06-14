using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Combat;

using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Network;
using LabFusion.Player;

using UnityEngine;

namespace LabFusion.Senders;

public enum PlayerActionType
{
    /// <summary>
    /// No event.
    /// </summary>
    UNKNOWN,

    /// <summary>
    /// Invoked when Player jumps.
    /// </summary>
    JUMP,

    /// <summary>
    /// Invoked when Player dies.
    /// </summary>
    DEATH,

    /// <summary>
    /// Invoked when Player plays the dying animation.
    /// </summary>
    DYING,

    /// <summary>
    /// Invoked when Player deals damage to Other Player.
    /// </summary>
    DEALT_DAMAGE_TO_OTHER_PLAYER,

    /// <summary>
    /// Invoked when Player plays the dying animation due to Other Player.
    /// </summary>
    DYING_BY_OTHER_PLAYER,

    /// <summary>
    /// Invoked when Player saves themselves before the dying animation ends.
    /// </summary>
    RECOVERY,

    /// <summary>
    /// Invoked when Player is killed by Other Player.
    /// </summary>
    DEATH_BY_OTHER_PLAYER,

    /// <summary>
    /// Invoked when Player respawns.
    /// </summary>
    RESPAWN,
}

public enum NicknameVisibility
{
    SHOW = 1 << 0,
    SHOW_WITH_PREFIX = 1 << 1,
    HIDE = 1 << 2,
}

public static class PlayerSender
{
    public static void SendPlayerAvatar(SerializedAvatarStats stats, string barcode)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = PlayerRepAvatarData.Create(PlayerIDManager.LocalSmallID, stats, barcode);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepAvatar, CommonMessageRoutes.ReliableToOtherClients);
    }

    public static void SendPlayerVoiceChat(byte[] voiceData)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = PlayerVoiceChatData.Create(PlayerIDManager.LocalSmallID, voiceData);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerVoiceChat, CommonMessageRoutes.UnreliableToOtherClients);
    }

    public static void SendPlayerTeleport(byte target, Vector3 position)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        var data = PlayerRepTeleportData.Create(target, position);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepTeleport, new MessageRoute(target, NetworkChannel.Reliable));
    }

    public static void SendPlayerDamage(byte target, Attack attack)
    {
        SendPlayerDamage(target, attack, PlayerDamageReceiver.BodyPart.Chest);
    }

    public static void SendPlayerDamage(byte target, Attack attack, PlayerDamageReceiver.BodyPart part)
    {
        var data = PlayerRepDamageData.Create(PlayerIDManager.LocalSmallID, target, attack, part);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepDamage, new MessageRoute(target, NetworkChannel.Reliable));
    }

    public static void SendPlayerMetadataRequest(byte smallId, string key, string value)
    {
        var data = PlayerMetadataRequestData.Create(smallId, key, value);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerMetadataRequest, CommonMessageRoutes.ReliableToServer);
    }

    public static void SendPlayerMetadataResponse(byte smallId, string key, string value)
    {
        // Make sure this is the server
        if (!NetworkInfo.IsHost)
        {
            throw new ExpectedServerException();
        }

        var data = PlayerMetadataResponseData.Create(smallId, key, value);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerMetadataResponse, CommonMessageRoutes.ReliableToClients);
    }

    public static void SendPlayerAction(PlayerActionType type, byte? otherPlayer = null)
    {
        var data = PlayerRepActionData.Create(PlayerIDManager.LocalSmallID, type, otherPlayer);

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerRepAction, CommonMessageRoutes.ReliableToClients);
    }
}