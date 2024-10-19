using LabFusion.Utilities;

using FusionHelper.Network;

using LiteNetLib.Utils;

namespace LabFusion.Network;

public static class ProxySocketHandler
{
    public static void BroadcastToClients(NetworkChannel channel, FusionMessage message)
    {
        MessageTypes type = channel == NetworkChannel.Reliable ? MessageTypes.ReliableBroadcastToClients : MessageTypes.UnreliableBroadcastToClients;
        NetDataWriter writer = ProxyNetworkLayer.NewWriter(type);
        byte[] data = message.ToByteArray();
        writer.PutBytesWithLength(data);
        ProxyNetworkLayer.Instance.SendToProxyServer(writer);
    }

    public static void BroadcastToServer(NetworkChannel channel, FusionMessage message)
    {
        try
        {
            MessageTypes type = channel == NetworkChannel.Reliable ? MessageTypes.ReliableBroadcastToServer : MessageTypes.UnreliableBroadcastToServer;
            NetDataWriter writer = ProxyNetworkLayer.NewWriter(type);
            byte[] data = message.ToByteArray();
            writer.PutBytesWithLength(data);
            ProxyNetworkLayer.Instance.SendToProxyServer(writer);
        }
        catch (Exception e)
        {
            FusionLogger.Error($"Failed sending message to socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
        }
    }

    public static void OnSocketMessageReceived(byte[] message, bool isServerHandled = false)
    {
        try
        {
            FusionMessageHandler.ReadMessage(new ReadOnlySpan<byte>(message), isServerHandled);
        }
        catch (Exception e)
        {
            FusionLogger.Error($"Failed reading message from socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
        }
    }
}