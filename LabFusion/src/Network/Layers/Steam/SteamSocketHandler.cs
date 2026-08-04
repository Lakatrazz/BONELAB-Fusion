using LabFusion.Utilities;

using Steamworks;
using Steamworks.Data;

namespace LabFusion.Network;

public static class SteamSocketHandler
{
    public static SendType ConvertToSendType(NetworkChannel channel)
    {
        var sendType = channel switch
        {
            NetworkChannel.Reliable => SendType.Reliable,
            _ => SendType.Unreliable,
        };
        return sendType;
    }

    public static void SendToClient(this SteamSocketManager socketManager, ClientPlatformID client, NetworkChannel channel, NetMessage message)
    {
        SendType sendType = ConvertToSendType(channel);
        int sizeOfMessage = message.Length;

        unsafe
        {
            if (!socketManager.ConnectedSteamIDs.TryGetValue((ulong)client, out var connection))
            {
                return;
            }

            connection.SendMessage((IntPtr)message.Buffer, sizeOfMessage, sendType);
        }
    }

    public static void ServerSendToClients(this SteamSocketManager socketManager, Span<ClientPlatformID> clients, NetworkChannel channel, NetMessage message)
    {
        SendType sendType = ConvertToSendType(channel);

        // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
        int sizeOfMessage = message.Length;

        unsafe
        {
            IntPtr messagePtr = (IntPtr)message.Buffer;

            foreach (var client in clients)
            {
                if (!socketManager.ConnectedSteamIDs.TryGetValue((ulong)client, out var connection))
                {
                    continue;
                }

                connection.SendMessage(messagePtr, sizeOfMessage, sendType);
            }
        }
    }

    public static void ClientSendToServer(this SteamConnectionManager connectionManager, NetworkChannel channel, NetMessage message)
    {
        try
        {
            SendType sendType = ConvertToSendType(channel);

            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = message.Length;

            unsafe
            {
                IntPtr messagePtr = (IntPtr)message.Buffer;
                Connection connection = connectionManager.Connection;

                Result success = connection.SendMessage(messagePtr, sizeOfMessage, sendType);

                if (success != Result.OK)
                {
                    Result retry = connection.SendMessage(messagePtr, sizeOfMessage, sendType);

                    if (retry != Result.OK)
                    {
                        throw new Exception($"Steam result was {retry}.");
                    }
                }
            }
        }
        catch (Exception e)
        {
            FusionLogger.LogException("sending message to socket server", e);
        }
    }

    public static void OnSocketMessageReceived(IntPtr messageIntPtr, int dataBlockSize, bool isServerHandled = false, ClientPlatformID? platformID = null)
    {
        try
        {
            unsafe
            {
                var messageSpan = new ReadOnlySpan<byte>(messageIntPtr.ToPointer(), dataBlockSize);

                var readableMessage = new ReadableMessage()
                {
                    Buffer = messageSpan,
                    IsServerHandled = isServerHandled,
                    PlatformID = platformID,
                };

                NativeMessageHandler.ReadMessage(readableMessage);
            }
        }
        catch (Exception e)
        {
            FusionLogger.Error($"Failed reading message from socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
        }
    }
}