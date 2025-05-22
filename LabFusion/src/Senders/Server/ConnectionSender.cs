using LabFusion.Data;
using LabFusion.Network;
using LabFusion.Network.Serialization;
using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Senders;

public static class ConnectionSender
{
    public static void SendDisconnectToAll(string reason = "")
    {
        if (NetworkInfo.IsHost)
        {
            foreach (var id in PlayerIdManager.PlayerIds)
            {
                if (id.IsMe)
                    continue;

                using var writer = NetWriter.Create();
                var disconnect = DisconnectMessageData.Create(id.LongId, reason);
                writer.SerializeValue(ref disconnect);

                using var message = FusionMessage.Create(NativeMessageTag.Disconnect, writer);
                MessageSender.SendFromServer(id.LongId, NetworkChannel.Reliable, message);
            }
        }
    }

    public static void SendDisconnect(ulong userId, string reason = "")
    {
        if (NetworkInfo.IsHost)
        {
            using var writer = NetWriter.Create();
            var disconnect = DisconnectMessageData.Create(userId, reason);
            writer.SerializeValue(ref disconnect);

            using var message = FusionMessage.Create(NativeMessageTag.Disconnect, writer);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
        }
    }

    public static void SendConnectionDeny(ulong userId, string reason = "")
    {
        if (NetworkInfo.IsHost)
        {
            using var writer = NetWriter.Create();
            var disconnect = DisconnectMessageData.Create(userId, reason);
            writer.SerializeValue(ref disconnect);

            using var message = FusionMessage.Create(NativeMessageTag.Disconnect, writer);
            MessageSender.SendFromServer(userId, NetworkChannel.Reliable, message);
        }
    }

    public static void SendConnectionRequest()
    {
        if (NetworkInfo.HasServer)
        {
            using var writer = NetWriter.Create();

            var data = ConnectionRequestData.Create(PlayerIdManager.LocalLongId, FusionMod.Version, RigData.GetAvatarBarcode(), RigData.RigAvatarStats);
            data.Serialize(writer);

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
        using var writer = NetWriter.Create();
        var response = ConnectionResponseData.Create(id, avatar, stats, false);
        writer.SerializeValue(ref response);

        using var message = FusionMessage.Create(NativeMessageTag.ConnectionResponse, writer);
        MessageSender.SendFromServer(newUser, NetworkChannel.Reliable, message);
    }

    public static void SendPlayerJoin(PlayerId id, string avatar, SerializedAvatarStats stats)
    {
        using var writer = NetWriter.Create();
        var response = ConnectionResponseData.Create(id, avatar, stats, true);
        writer.SerializeValue(ref response);

        using var message = FusionMessage.Create(NativeMessageTag.ConnectionResponse, writer);
        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
    }
}