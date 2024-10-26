using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Senders;

public static class ConnectionSender
{
    public static void SendDisconnectToAll(string reason = "")
    {
        if (NetworkInfo.IsServer)
        {
            foreach (var id in PlayerIdManager.PlayerIds)
            {
                if (id.IsMe)
                    continue;

                using FusionWriter writer = FusionWriter.Create();
                var disconnect = DisconnectMessageData.Create(id.LongId, reason);
                writer.Write(disconnect);

                using var message = FusionMessage.Create(NativeMessageTag.Disconnect, writer);
                MessageSender.SendFromServer(id.LongId, NetworkChannel.Reliable, message);
            }
        }
    }

    public static void SendDisconnect(ulong userId, string reason = "")
    {
        if (NetworkInfo.IsServer)
        {
            using FusionWriter writer = FusionWriter.Create();
            var disconnect = DisconnectMessageData.Create(userId, reason);
            writer.Write(disconnect);

            using var message = FusionMessage.Create(NativeMessageTag.Disconnect, writer);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
        }
    }

    public static void SendConnectionDeny(ulong userId, string reason = "")
    {
        if (NetworkInfo.IsServer)
        {
            using FusionWriter writer = FusionWriter.Create();
            var disconnect = DisconnectMessageData.Create(userId, reason);
            writer.Write(disconnect);

            using var message = FusionMessage.Create(NativeMessageTag.Disconnect, writer);
            MessageSender.SendFromServer(userId, NetworkChannel.Reliable, message);
        }
    }

    public static void SendConnectionRequest()
    {
        if (NetworkInfo.HasServer)
        {
            using FusionWriter writer = FusionWriter.Create();
            var data = ConnectionRequestData.Create(PlayerIdManager.LocalLongId, FusionMod.Version, RigData.GetAvatarBarcode(), RigData.RigAvatarStats);
            writer.Write(data);

            using FusionMessage message = FusionMessage.Create(NativeMessageTag.ConnectionRequest, writer);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
        }
        else
        {
            FusionLogger.Error("Attempted to send a connection request, but we are not connected to anyone!");
        }
    }

    public static void SendPlayerCatchup(ulong newUser, PlayerId id, string avatar, SerializedAvatarStats stats)
    {
        using FusionWriter writer = FusionWriter.Create();
        var response = ConnectionResponseData.Create(id, avatar, stats, false);
        writer.Write(response);

        using var message = FusionMessage.Create(NativeMessageTag.ConnectionResponse, writer);
        MessageSender.SendFromServer(newUser, NetworkChannel.Reliable, message);
    }

    public static void SendPlayerJoin(PlayerId id, string avatar, SerializedAvatarStats stats)
    {
        using FusionWriter writer = FusionWriter.Create();
        var response = ConnectionResponseData.Create(id, avatar, stats, true);
        writer.Write(response);

        using var message = FusionMessage.Create(NativeMessageTag.ConnectionResponse, writer);
        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
    }
}