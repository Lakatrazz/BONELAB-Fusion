using LabFusion.Utilities;

using LiteNetLib.Utils;

namespace LabFusion.Network.Proxy;

public static class ProxySocketHandler
{
    public static void BroadcastToClients(NetworkChannel channel, NetMessage message)
    {
        MessageTypes type = channel == NetworkChannel.Reliable ? MessageTypes.ReliableBroadcastToClients : MessageTypes.UnreliableBroadcastToClients;
        NetDataWriter writer = ProxyNetworkLayer.NewWriter(type);
        byte[] data = message.ToByteArray();
        writer.PutBytesWithLength(data);
        ProxyNetworkLayer.Instance.SendToProxyServer(writer);
    }

    public static void BroadcastToServer(NetworkChannel channel, NetMessage message)
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
            var readableMessage = new ReadableMessage()
            {
                Buffer = new ReadOnlySpan<byte>(message),
                IsServerHandled = isServerHandled,
            };

            NativeMessageHandler.ReadMessage(readableMessage);
        }
        catch (Exception e)
        {
            FusionLogger.Error($"Failed reading message from socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
        }
    }
}