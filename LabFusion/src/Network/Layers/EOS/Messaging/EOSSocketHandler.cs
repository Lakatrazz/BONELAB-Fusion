using Epic.OnlineServices;

using LabFusion.Utilities;

using System.Collections.Concurrent;

namespace LabFusion.Network;

internal static class EOSSocketHandler
{
    private const int MAX_EOS_PACKET_SIZE = 1170;
    private const int MAX_MESSAGES_PER_FRAME = 100;
    private const int POOL_MAX_SIZE = 100;

    private const byte SERVER_CHANNEL = 2;
    private const byte CLIENT_CHANNEL = 1;

    private static readonly ConcurrentQueue<byte[]> _bufferPool = new();

    internal static Epic.OnlineServices.P2P.SocketId SocketId = new Epic.OnlineServices.P2P.SocketId
    {
        SocketName = "FusionSocket"
    };

    internal static void ConfigureP2P() => EOSConnectionManager.ConfigureP2P();

    internal static void BroadcastToServer(NetworkChannel channel, NetMessage message) => EOSMessageSender.BroadcastToServer(channel, message);

    internal static void BroadcastToClients(NetworkChannel channel, NetMessage message) => EOSMessageSender.BroadcastToClients(channel, message);

    internal static void SendFromServer(string userId, NetworkChannel channel, NetMessage message) => EOSMessageSender.SendFromServer(userId, channel, message);

    internal static void CleanupOldFragments() => PacketFragmentation.CleanupOldFragments();

    private static byte[] RentBuffer(int size)
    {
        if (_bufferPool.TryDequeue(out var buffer) && buffer.Length >= size)
            return buffer;
        return new byte[size];
    }

    private static void ReturnBuffer(byte[] buffer)
    {
        if (_bufferPool.Count < POOL_MAX_SIZE && buffer != null)
            _bufferPool.Enqueue(buffer);
    }

    internal static Result SendPacketToUser(ProductUserId userId, NetMessage message, NetworkChannel channel, bool isServerHandled)
    {
        byte[] data = message.ToByteArray();

        // Use fragmentation for large packets
        if (data.Length > MAX_EOS_PACKET_SIZE)
        {
            return PacketFragmentation.SendFragmentedPacket(userId, data, channel, isServerHandled, MAX_EOS_PACKET_SIZE);
        }

        return SendSinglePacket(userId, data, channel, isServerHandled);
    }

    internal static void ReceiveMessages()
    {
        try
        {
            var localUserId = EOSNetworkLayer.LocalUserId;

            var getPacketSizeOptions = new Epic.OnlineServices.P2P.GetNextReceivedPacketSizeOptions
            {
                LocalUserId = localUserId,
                RequestedChannel = null
            };

            var receiveOptions = new Epic.OnlineServices.P2P.ReceivePacketOptions
            {
                LocalUserId = localUserId,
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

    private static Result SendSinglePacket(ProductUserId userId, byte[] data, NetworkChannel channel, bool isServerHandled, bool voice = false)
    {
        var sendOptions = new Epic.OnlineServices.P2P.SendPacketOptions()
        {
            LocalUserId = EOSNetworkLayer.LocalUserId,
            RemoteUserId = userId,
            SocketId = SocketId,
            Channel = isServerHandled ? SERVER_CHANNEL : CLIENT_CHANNEL,
            Data = new ArraySegment<byte>(data),
            AllowDelayedDelivery = false,
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

        if (packetSize == 0)
            return false;

        var buffer = RentBuffer((int)packetSize);
        var dataSegment = new ArraySegment<byte>(buffer);
        options.MaxDataSizeBytes = packetSize;

        ProductUserId peerId = null;
        var result = EOSManager.P2PInterface.ReceivePacket(ref options, ref peerId, ref SocketId, out byte channel, dataSegment, out uint bytesWritten);

        if (result != Result.Success)
        {
            FusionLogger.Error($"Failed to receive packet: {result}");
            ReturnBuffer(buffer);
            return false;
        }

        if (bytesWritten == 0 || peerId == null)
        {
            ReturnBuffer(buffer);
            return false;
        }

        NetworkInfo.LastReceivedUser = peerId.ToString();
        packetData = new ReceivedPacketData(buffer, (int)bytesWritten, peerId, channel == SERVER_CHANNEL);
        return true;
    }

    private static void ProcessReceivedPacket(ReceivedPacketData packetData)
    {
        ReadOnlySpan<byte> messageBuffer;

        if (PacketFragmentation.IsFragment(packetData.Buffer))
        {
            if (!PacketFragmentation.TryHandleFragment(packetData.Buffer, packetData.BytesWritten, packetData.PeerId, out byte[] reassembledData))
            {
                ReturnBuffer(packetData.Buffer);
                return; // Incomplete fragment
            }
            messageBuffer = reassembledData;
            ReturnBuffer(packetData.Buffer);
        }
        else
        {
            messageBuffer = packetData.BufferSegment;
        }

        try
        {
            NativeMessageHandler.ReadMessage(new ReadableMessage
            {
                Buffer = messageBuffer,
                IsServerHandled = packetData.IsServerHandled
            });
        }
        finally
        {
            if (!PacketFragmentation.IsFragment(packetData.Buffer))
                ReturnBuffer(packetData.Buffer);
        }
    }

    private readonly struct ReceivedPacketData
    {
        public readonly ArraySegment<byte> BufferSegment;
        public readonly ProductUserId PeerId;
        public readonly bool IsServerHandled;

        public byte[] Buffer => BufferSegment.Array;
        public int BytesWritten => BufferSegment.Count;

        public ReceivedPacketData(byte[] buffer, int bytesWritten, ProductUserId peerId, bool isServerHandled)
        {
            BufferSegment = new ArraySegment<byte>(buffer, 0, bytesWritten);
            PeerId = peerId;
            IsServerHandled = isServerHandled;
        }
    }
}