using LabFusion.Player;

namespace LabFusion.Network;

/// <summary>
/// Helper class for sending messages to the server, or to other users if this is the server.
/// </summary>
public static class MessageSender
{
    /// <summary>
    /// Sends the message to the specified user if this is a server.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public static void SendFromServer(byte userId, NetworkChannel channel, NetMessage message)
    {
        if (message == null)
            return;

        if (NetworkLayerManager.Layer != null)
        {
            NetworkInfo.BytesUp += message.Length;

            NetworkLayerManager.Layer.SendFromServer(userId, channel, message);
        }
    }

    /// <summary>
    /// Sends the message to the specified user if this is a server.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public static void SendFromServer(ulong userId, NetworkChannel channel, NetMessage message)
    {
        if (message == null)
        {
            return;
        }

        if (NetworkLayerManager.Layer != null)
        {
            NetworkInfo.BytesUp += message.Length;

            NetworkLayerManager.Layer.SendFromServer(userId, channel, message);
        }
    }

    /// <summary>
    /// Sends the message to the dedicated server.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public static void SendToServer(NetworkChannel channel, NetMessage message)
    {
        if (message == null)
        {
            return;
        }

        if (NetworkLayerManager.Layer != null)
        {
            NetworkInfo.BytesUp += message.Length;

            if (!NetworkInfo.IsHost)
            {
                NetworkLayerManager.Layer.SendToServer(channel, message);
            }
            else
            {
                unsafe
                {
                    NetworkInfo.LastReceivedUser = PlayerIDManager.LocalPlatformID;

                    var readableMessage = new ReadableMessage()
                    {
                        Buffer = new ReadOnlySpan<byte>(message.Buffer, message.Length),
                        IsServerHandled = true,
                    };

                    NativeMessageHandler.ReadMessage(readableMessage);
                }
            }
        }
    }

    /// <summary>
    /// Sends the message to the server if this is a client. Sends to all clients if this is a server.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public static void BroadcastMessage(NetworkChannel channel, NetMessage message)
    {
        if (message == null)
            return;

        if (NetworkLayerManager.Layer != null)
        {
            NetworkInfo.BytesUp += message.Length;

            NetworkLayerManager.Layer.BroadcastMessage(channel, message);

            // Backup incase the message cannot be sent to the host, which this targets.
            if (!NetworkInfo.ServerCanSendToHost && NetworkInfo.IsHost)
            {
                unsafe
                {
                    NetworkInfo.LastReceivedUser = PlayerIDManager.LocalPlatformID;

                    var readableMessage = new ReadableMessage()
                    {
                        Buffer = new ReadOnlySpan<byte>(message.Buffer, message.Length),
                        IsServerHandled = false,
                    };

                    NativeMessageHandler.ReadMessage(readableMessage);
                }
            }
        }
    }

    /// <summary>
    /// If this is a server, sends this message back to all users except for the provided id.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public static void BroadcastMessageExcept(byte userId, NetworkChannel channel, NetMessage message, bool ignoreHost = true)
    {
        if (message == null)
            return;

        if (NetworkLayerManager.Layer != null)
        {
            NetworkInfo.BytesUp += message.Length;

            NetworkLayerManager.Layer.BroadcastMessageExcept(userId, channel, message, ignoreHost);

            // Backup incase the message cannot be sent to the host, which this targets.
            if (!ignoreHost && userId != PlayerIDManager.LocalSmallID && !NetworkInfo.ServerCanSendToHost && NetworkInfo.IsHost)
            {
                unsafe
                {
                    NetworkInfo.LastReceivedUser = PlayerIDManager.LocalPlatformID;

                    var readableMessage = new ReadableMessage()
                    {
                        Buffer = new ReadOnlySpan<byte>(message.Buffer, message.Length),
                        IsServerHandled = false,
                    };

                    NativeMessageHandler.ReadMessage(readableMessage);
                }
            }
        }
    }

    /// <summary>
    /// If this is a server, sends this message back to all users except for the provided id.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public static void BroadcastMessageExcept(ulong userId, NetworkChannel channel, NetMessage message, bool ignoreHost = true)
    {
        if (message == null)
            return;

        if (NetworkLayerManager.Layer != null)
        {
            NetworkInfo.BytesUp += message.Length;

            NetworkLayerManager.Layer.BroadcastMessageExcept(userId, channel, message, ignoreHost);

            // Backup incase the message cannot be sent to the host, which this targets.
            if (!ignoreHost && userId != PlayerIDManager.LocalPlatformID && !NetworkInfo.ServerCanSendToHost && NetworkInfo.IsHost)
            {
                unsafe
                {
                    NetworkInfo.LastReceivedUser = PlayerIDManager.LocalPlatformID;

                    var readableMessage = new ReadableMessage()
                    {
                        Buffer = new ReadOnlySpan<byte>(message.Buffer, message.Length),
                        IsServerHandled = false,
                    };

                    NativeMessageHandler.ReadMessage(readableMessage);
                }
            }
        }
    }

    /// <summary>
    /// Sends the message to the server if this is a client. Sends to all clients EXCEPT THE HOST if this is a server.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public static void BroadcastMessageExceptSelf(NetworkChannel channel, NetMessage message)
    {
        if (message == null)
            return;

        if (NetworkInfo.IsHost)
        {
            BroadcastMessageExcept(0, channel, message);
        }
        else
        {
            BroadcastMessage(channel, message);
        }
    }
}