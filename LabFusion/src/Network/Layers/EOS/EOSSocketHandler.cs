using Epic.OnlineServices;
using LabFusion.Utilities;

namespace LabFusion.Network;

internal static class EOSSocketHandler
{
    private const int MAX_EOS_PACKET_SIZE = 1170;
    private const int MAX_MESSAGES_PER_FRAME = 100;

    internal static Epic.OnlineServices.P2P.SocketId SocketId = new Epic.OnlineServices.P2P.SocketId 
    { 
        SocketName = "FusionSocket" 
    };

    internal static void ConfigureP2P() => EOSConnectionManager.ConfigureP2P();

    internal static void CloseConnections() => EOSConnectionManager.CloseConnections();

    internal static void BroadcastToServer(NetworkChannel channel, NetMessage message) => EOSMessageSender.BroadcastToServer(channel, message);

    internal static void BroadcastToClients(NetworkChannel channel, NetMessage message) => EOSMessageSender.BroadcastToClients(channel, message);

    internal static void SendFromServer(string userId, NetworkChannel channel, NetMessage message) => EOSMessageSender.SendFromServer(userId, channel, message);

    internal static void CleanupOldFragments() => PacketFragmentation.CleanupOldFragments();

    internal static Result SendPacketToUser(ProductUserId userId, byte[] data, NetworkChannel channel, bool isServerHandled)
    {
        // Handle local packets with frame delay to avoid issues as host
        if (userId == EOSNetworkLayer.LocalUserId)
        {
            DelayUtilities.InvokeNextFrame(() => {
                var readableMessage = new ReadableMessage()
                {
                    Buffer = new ReadOnlySpan<byte>(data),
                    IsServerHandled = isServerHandled
                };
                NativeMessageHandler.ReadMessage(readableMessage);
            });
            return Result.Success;
        }

        // Use fragmentation for large packets
        if (data.Length > MAX_EOS_PACKET_SIZE)
        {
            return PacketFragmentation.SendFragmentedPacket(userId, data, channel, isServerHandled, MAX_EOS_PACKET_SIZE);
        }

        return SendSinglePacket(userId, data, channel, isServerHandled);
    }

    internal static unsafe void ReceiveMessages()
    {
        try
        {
            var getPacketSizeOptions = new Epic.OnlineServices.P2P.GetNextReceivedPacketSizeOptions
            {
                LocalUserId = EOSNetworkLayer.LocalUserId,
                RequestedChannel = null
            };

            var receiveOptions = new Epic.OnlineServices.P2P.ReceivePacketOptions
            {
                LocalUserId = EOSNetworkLayer.LocalUserId,
                RequestedChannel = null
            };

            for (int i = 0; i < MAX_MESSAGES_PER_FRAME; i++)
            {
                if (!TryGetNextPacketSize(ref getPacketSizeOptions, out uint packetSize))
                    break;

                if (!TryReceivePacket(ref receiveOptions, packetSize, out var packetData))
                    break;

                ProcessReceivedPacket(packetData);
            }
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("Error receiving P2P messages", ex);
        }
    }

    private static Result SendSinglePacket(ProductUserId userId, byte[] data, NetworkChannel channel, bool isServerHandled)
    {
        var sendOptions = new Epic.OnlineServices.P2P.SendPacketOptions()
        {
            LocalUserId = EOSNetworkLayer.LocalUserId,
            RemoteUserId = userId,
            SocketId = SocketId,
            Channel = isServerHandled ? (byte)2 : (byte)1,
            Data = new ArraySegment<byte>(data),
            AllowDelayedDelivery = true,
            Reliability = channel == NetworkChannel.Reliable
                ? Epic.OnlineServices.P2P.PacketReliability.ReliableUnordered
                : Epic.OnlineServices.P2P.PacketReliability.UnreliableUnordered,
            DisableAutoAcceptConnection = false,
        };

        return EOSManager.P2PInterface.SendPacket(ref sendOptions);
    }

    private static bool TryGetNextPacketSize(ref Epic.OnlineServices.P2P.GetNextReceivedPacketSizeOptions options, out uint packetSize)
    {
        return EOSManager.P2PInterface.GetNextReceivedPacketSize(ref options, out packetSize) == Result.Success;
    }

    private static bool TryReceivePacket(ref Epic.OnlineServices.P2P.ReceivePacketOptions options, uint packetSize, out ReceivedPacketData packetData)
    {
        packetData = default;

        byte[] buffer = new byte[packetSize];
        var dataSegment = new ArraySegment<byte>(buffer);
        options.MaxDataSizeBytes = packetSize;

        ProductUserId peerId = null;
        Result result = EOSManager.P2PInterface.ReceivePacket(ref options, ref peerId, ref SocketId, out byte channel, dataSegment, out uint bytesWritten);

        if (result == Result.Success && bytesWritten > 0)
        {
            if (peerId != null)
            {
                NetworkInfo.LastReceivedUser = peerId.ToString();
            }

            packetData = new ReceivedPacketData
            {
                Buffer = buffer,
                BytesWritten = (int)bytesWritten,
                PeerId = peerId,
                IsServerHandled = channel == 2
            };
            return true;
        }

        if (result != Result.Success)
        {
            FusionLogger.Error($"Failed to receive packet: {result}");
        }

        return false;
    }

    private static unsafe void ProcessReceivedPacket(ReceivedPacketData packetData)
    {
        // Try to handle as fragment first
        if (PacketFragmentation.TryHandleFragment(packetData.Buffer, packetData.BytesWritten, packetData.PeerId, out byte[] reassembledData))
        {
            var readableMessage = new ReadableMessage()
            {
                Buffer = new ReadOnlySpan<byte>(reassembledData),
                IsServerHandled = packetData.IsServerHandled
            };
            NativeMessageHandler.ReadMessage(readableMessage);
        }
        else if (!PacketFragmentation.IsFragment(packetData.Buffer))
        {
            // Process as regular message if not a fragment
            var readableMessage = new ReadableMessage()
            {
                Buffer = new ReadOnlySpan<byte>(packetData.Buffer, 0, packetData.BytesWritten),
                IsServerHandled = packetData.IsServerHandled
            };
            NativeMessageHandler.ReadMessage(readableMessage);
        }
        // If its a fragment but not complete, just ignore
    }

    private struct ReceivedPacketData
    {
        public byte[] Buffer;
        public int BytesWritten;
        public ProductUserId PeerId;
        public bool IsServerHandled;
    }
}