using Il2CppSLZ.Marrow;
using Il2CppSLZ.Marrow.Combat;

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
    public static void SendPlayerVoiceChat(byte[] voiceData)
    {
        if (!NetworkManager.HasServer)
        {
            return;
        }

        var data = new PlayerVoiceChatData()
        {
            Bytes = voiceData,
        };

        ClientManager.RelayNative(data, NativeMessageTag.PlayerVoiceChat, CommonMessageRoutes.UnreliableToOtherClients);
    }

    public static void SendPlayerTeleport(ClientSmallID target, Vector3 position)
    {
        if (!ServerManager.IsServerRunning)
        {
            return;
        }

        var data = new RigTeleportData()
        {
            RigReference = new((ushort)target),
            Position = position,
        };

        if (!PlayerIDManager.TryGetPlatformID(target, out var targetPlatformID))
        {
            return;
        }

        ServerManager.SendToClientModule<RigTeleportMessage, RigTeleportData>(data, NetworkChannel.Reliable, targetPlatformID);
    }

    public static void SendPlayerDamage(ClientSmallID target, Attack attack)
    {
        SendPlayerDamage(target, attack, PlayerDamageReceiver.BodyPart.Chest);
    }

    public static void SendPlayerDamage(ClientSmallID target, Attack attack, PlayerDamageReceiver.BodyPart part)
    {
        // TODO: Make work for all owned rigs
        var data = new RigDamageData()
        {
            RigReference = new((ushort)target),
            Attack = new(attack),
            Part = part
        };

        ClientManager.RelayModule<RigDamageMessage, RigDamageData>(data, new MessageRoute(target, NetworkChannel.Reliable));
    }

    public static void SendPlayerMetadataRequest(ClientSmallID smallID, string key, string value)
    {
        var data = new PlayerMetadataData()
        {
            Player = new(smallID),
            Key = key,
            Value = value,
        };

        ClientManager.RelayNative(data, NativeMessageTag.PlayerMetadataRequest, CommonMessageRoutes.ReliableToServer);
    }

    public static void SendPlayerMetadataResponse(ClientSmallID smallID, string key, string value)
    {
        // Make sure this is the server
        if (!ServerManager.IsServerRunning)
        {
            throw new MessageExpectedServerException();
        }

        var data = new PlayerMetadataData()
        {
            Player = new(smallID),
            Key = key,
            Value = value,
        };

        ServerManager.SendToClientsNative(data, NativeMessageTag.PlayerMetadataResponse, NetworkChannel.Reliable);
    }
}