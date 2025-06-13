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
            foreach (var id in PlayerIDManager.PlayerIDs)
            {
                if (id.IsMe)
                    continue;

                using var writer = NetWriter.Create();
                var disconnect = DisconnectMessageData.Create(id.PlatformID, reason);
                writer.SerializeValue(ref disconnect);

                using var message = NetMessage.Create(NativeMessageTag.Disconnect, writer, CommonMessageRoutes.None);
                MessageSender.SendFromServer(id.PlatformID, NetworkChannel.Reliable, message);
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

            using var message = NetMessage.Create(NativeMessageTag.Disconnect, writer, CommonMessageRoutes.None);
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

            using var message = NetMessage.Create(NativeMessageTag.Disconnect, writer, CommonMessageRoutes.None);
            MessageSender.SendFromServer(userId, NetworkChannel.Reliable, message);
        }
    }

    public static void SendConnectionRequest()
    {
        if (NetworkInfo.HasServer)
        {
            using var writer = NetWriter.Create();

            var data = ConnectionRequestData.Create(PlayerIDManager.LocalPlatformID, FusionMod.Version, RigData.GetAvatarBarcode(), RigData.RigAvatarStats);
            data.Serialize(writer);

            using NetMessage message = NetMessage.Create(NativeMessageTag.ConnectionRequest, writer, CommonMessageRoutes.None);
            MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
        }
        else
        {
            FusionLogger.Error("Attempted to send a connection request, but we are not connected to anyone!");
        }
    }

    public static void SendPlayerCatchup(ulong newUser, PlayerID id, string avatar, SerializedAvatarStats stats)
    {
        using var writer = NetWriter.Create();
        var response = ConnectionResponseData.Create(id, avatar, stats, false);
        writer.SerializeValue(ref response);

        using var message = NetMessage.Create(NativeMessageTag.ConnectionResponse, writer, CommonMessageRoutes.None);
        MessageSender.SendFromServer(newUser, NetworkChannel.Reliable, message);
    }

    public static void SendPlayerJoin(PlayerID id, string avatar, SerializedAvatarStats stats)
    {
        using var writer = NetWriter.Create();
        var response = ConnectionResponseData.Create(id, avatar, stats, true);
        writer.SerializeValue(ref response);

        using var message = NetMessage.Create(NativeMessageTag.ConnectionResponse, writer, CommonMessageRoutes.None);
        MessageSender.BroadcastMessage(NetworkChannel.Reliable, message);
    }
}