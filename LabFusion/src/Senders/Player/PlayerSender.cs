using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Combat;

using LabFusion.Data;
using LabFusion.Exceptions;
using LabFusion.Marrow.Messages;
using LabFusion.Network;
using LabFusion.Player;

using UnityEngine;

namespace LabFusion.Senders;

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

        // TODO: Move to NetworkRig
        var data = new RigAvatarData()
        {
            RigReference = new(PlayerIDManager.LocalSmallID),
            Stats = stats,
            Barcode = barcode,
        };

        MessageRelay.RelayModule<RigAvatarMessage, RigAvatarData>(data, CommonMessageRoutes.ReliableToOtherClients);
    }

    public static void SendPlayerVoiceChat(byte[] voiceData)
    {
        if (!NetworkInfo.HasServer)
        {
            return;
        }

        var data = new PlayerVoiceChatData()
        {
            Bytes = voiceData,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerVoiceChat, CommonMessageRoutes.UnreliableToOtherClients);
    }

    public static void SendPlayerTeleport(byte target, Vector3 position)
    {
        if (!NetworkInfo.IsHost)
        {
            return;
        }

        var data = new RigTeleportData()
        {
            RigReference = new(target),
            Position = position,
        };

        MessageRelay.RelayModule<RigTeleportMessage, RigTeleportData>(data, new MessageRoute(target, NetworkChannel.Reliable));
    }

    public static void SendPlayerDamage(byte target, Attack attack)
    {
        SendPlayerDamage(target, attack, PlayerDamageReceiver.BodyPart.Chest);
    }

    public static void SendPlayerDamage(byte target, Attack attack, PlayerDamageReceiver.BodyPart part)
    {
        // TODO: Make work for all owned rigs
        var data = new RigDamageData()
        {
            RigReference = new(target),
            Attack = new(attack),
            Part = part
        };

        MessageRelay.RelayModule<RigDamageMessage, RigDamageData>(data, new MessageRoute(target, NetworkChannel.Reliable));
    }

    public static void SendPlayerMetadataRequest(byte smallID, string key, string value)
    {
        var data = new PlayerMetadataData()
        {
            Player = new(smallID),
            Key = key,
            Value = value,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerMetadataRequest, CommonMessageRoutes.ReliableToServer);
    }

    public static void SendPlayerMetadataResponse(byte smallID, string key, string value)
    {
        // Make sure this is the server
        if (!NetworkInfo.IsHost)
        {
            throw new MessageExpectedServerException();
        }

        var data = new PlayerMetadataData()
        {
            Player = new(smallID),
            Key = key,
            Value = value,
        };

        MessageRelay.RelayNative(data, NativeMessageTag.PlayerMetadataResponse, CommonMessageRoutes.ReliableToClients);
    }
}