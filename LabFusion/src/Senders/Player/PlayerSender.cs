using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Combat;

using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Extensions;
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
            return;

        using FusionWriter writer = FusionWriter.Create(PlayerRepAvatarData.DefaultSize + barcode.GetSize());
        PlayerRepAvatarData data = PlayerRepAvatarData.Create(PlayerIdManager.LocalSmallId, stats, barcode);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.PlayerRepAvatar, writer);
        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Reliable, message);
    }

    public static void SendPlayerVoiceChat(byte[] voiceData)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        using var writer = FusionWriter.Create(PlayerVoiceChatData.Size + voiceData.Length);
        using var data = PlayerVoiceChatData.Create(PlayerIdManager.LocalSmallId, voiceData);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.PlayerVoiceChat, writer);
        MessageSender.BroadcastMessageExceptSelf(NetworkChannel.Unreliable, message);
    }

    public static void SendPlayerTeleport(byte target, Vector3 position)
    {
        if (!NetworkInfo.IsServer)
        {
            return;
        }

        using var writer = FusionWriter.Create(PlayerRepTeleportData.Size);
        var data = PlayerRepTeleportData.Create(target, position);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.PlayerRepTeleport, writer);
        MessageSender.SendFromServer(target, NetworkChannel.Reliable, message);
    }

    public static void SendPlayerDamage(byte target, Attack attack)
    {
        SendPlayerDamage(target, attack, PlayerDamageReceiver.BodyPart.Chest);
    }

    public static void SendPlayerDamage(byte target, Attack attack, PlayerDamageReceiver.BodyPart part)
    {
        using var writer = FusionWriter.Create(PlayerRepDamageData.Size);
        var data = PlayerRepDamageData.Create(PlayerIdManager.LocalSmallId, target, attack, part);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.PlayerRepDamage, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }

    public static void SendPlayerMetadataRequest(byte smallId, string key, string value)
    {
        using var writer = FusionWriter.Create(PlayerMetadataRequestData.GetSize(key, value));
        var data = PlayerMetadataRequestData.Create(smallId, key, value);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.PlayerMetadataRequest, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }

    public static void SendPlayerMetadataResponse(byte smallId, string key, string value)
    {
        // Make sure this is the server
        if (NetworkInfo.IsServer)
        {
            using var writer = FusionWriter.Create(PlayerMetadataResponseData.GetSize(key, value));
            var data = PlayerMetadataResponseData.Create(smallId, key, value);
            writer.Write(data);

            using var message = FusionMessage.Create(NativeMessageTag.PlayerMetadataResponse, writer);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
        }
        else
            throw new ExpectedClientException();
    }

    public static void SendPlayerAction(PlayerActionType type, byte? otherPlayer = null)
    {
        using var writer = FusionWriter.Create(PlayerRepActionData.Size);
        var data = PlayerRepActionData.Create(PlayerIdManager.LocalSmallId, type, otherPlayer);
        writer.Write(data);

        using var message = FusionMessage.Create(NativeMessageTag.PlayerRepAction, writer);
        MessageSender.SendToServer(NetworkChannel.Reliable, message);
    }
}