using System.Collections.Concurrent;
using System.Buffers.Binary;

using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Epic.OnlineServices.P2P;

using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Network.EpicGames;

internal static class EOSMessenger
{
    private const int MAX_EOS_PACKET_SIZE = 1170;
    private const int MAX_MESSAGES_PER_FRAME = 100;
    private const int POOL_MAX_SIZE = 100;
    private const byte SERVER_CHANNEL = 2;
    private const byte CLIENT_CHANNEL = 1;
    
    // Fragmentation constants
    private const int HEADER_SIZE = 8;
    private const ushort MAGIC_MARKER = 0xF2A9;
    private const int MAX_FRAGMENTS = 1000;
    private const int FRAGMENT_CLEANUP_SECONDS = 30;
    
    private static readonly ConcurrentQueue<byte[]> _bufferPool = new();
    private static readonly Dictionary<(string, ushort), FragmentCollection> _incomingFragments = new();
    private static int _nextFragmentId = 1;

    internal static SocketId SocketId = new SocketId { SocketName = "FusionSocket" };

    private struct FragmentCollection
    {
        public byte[][] Fragments;
        public bool[] ReceivedFlags;
        public int ReceivedCount;
        public int TotalSize;
        public DateTime LastReceived;
    }

    internal static void ReceiveMessages()
    {
        try
        {
            var localUserId = ProductUserId.FromString(PlayerIDManager.LocalPlatformID);

            var getPacketSizeOptions = new GetNextReceivedPacketSizeOptions
            {
                LocalUserId = localUserId,
                RequestedChannel = null
            };

            var receiveOptions = new ReceivePacketOptions
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

    internal static void BroadcastToClients(NetworkChannel channel, NetMessage message)
    {
        LobbyDetailsGetMemberCountOptions lobbyDetailsGetMemberCountOptions = new LobbyDetailsGetMemberCountOptions();
        uint memberCount = EpicLobby.LobbyDetails.GetMemberCount(ref lobbyDetailsGetMemberCountOptions);
        
        for (uint i = 0; i < memberCount; i++)
        {
            var lobbyDetailsGetMemberByIndexOptions = new LobbyDetailsGetMemberByIndexOptions { MemberIndex = i };
            var memberId = EpicLobby.LobbyDetails.GetMemberByIndex(ref lobbyDetailsGetMemberByIndexOptions);
            SendPacket(memberId, message, channel, false);
        }
    }

    internal static void BroadcastToServer(NetworkChannel channel, NetMessage message)
    { 
        if (PlayerIDManager.GetHostID() != null)
            SendPacket(ProductUserId.FromString(PlayerIDManager.GetHostID().PlatformID), message, channel, true);
        else 
        {
            LobbyDetailsGetLobbyOwnerOptions lobbyDetailsGetLobbyOwnerOptions = new LobbyDetailsGetLobbyOwnerOptions();
            ProductUserId hostId = EpicLobby.LobbyDetails.GetLobbyOwner(ref lobbyDetailsGetLobbyOwnerOptions);
            
            SendPacket(hostId, message, channel, true);
        }
    }
    
    internal static void SendFromServer(string userId, NetworkChannel channel, NetMessage message)
    {
        SendPacket(ProductUserId.FromString(userId), message, channel, false);
    }

    internal static void CleanupOldFragments()
    {
        var cutoffTime = DateTime.UtcNow.AddSeconds(-FRAGMENT_CLEANUP_SECONDS);
        var keysToRemove = _incomingFragments
            .Where(kvp => kvp.Value.LastReceived < cutoffTime)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
            _incomingFragments.Remove(key);
    }

    internal static Result SendPacket(ProductUserId userId, NetMessage message, NetworkChannel channel, bool isServerHandled)
    {
        byte[] data = message.ToByteArray();
        
        if (data.Length > MAX_EOS_PACKET_SIZE)
            return SendFragmented(userId, data, channel, isServerHandled);
        
        return SendSingle(userId, data, channel, isServerHandled);
    }

    private static Result SendSingle(ProductUserId userId, byte[] data, NetworkChannel channel, bool isServerHandled)
    {
        var options = new SendPacketOptions
        {
            LocalUserId = ProductUserId.FromString(PlayerIDManager.LocalPlatformID),
            RemoteUserId = userId,
            SocketId = SocketId,
            Channel = isServerHandled ? SERVER_CHANNEL : CLIENT_CHANNEL,
            Data = new ArraySegment<byte>(data),
            AllowDelayedDelivery = false,
            Reliability = channel == NetworkChannel.Reliable 
                ? PacketReliability.ReliableUnordered 
                : PacketReliability.UnreliableUnordered,
            DisableAutoAcceptConnection = false
        };
        
        return EOSManager.P2PInterface.SendPacket(ref options);
    }

    private static Result SendFragmented(ProductUserId userId, byte[] data, NetworkChannel channel, bool isServerHandled)
    {
        int maxDataPerFragment = MAX_EOS_PACKET_SIZE - HEADER_SIZE;
        int totalFragments = (data.Length + maxDataPerFragment - 1) / maxDataPerFragment;

        if (totalFragments > MAX_FRAGMENTS)
        {
            FusionLogger.Error($"Message too large: {data.Length} bytes would create {totalFragments} fragments");
            return Result.InvalidParameters;
        }

        ushort fragmentId = (ushort)Interlocked.Increment(ref _nextFragmentId);

        var options = new SendPacketOptions
        {
            LocalUserId = ProductUserId.FromString(PlayerIDManager.LocalPlatformID),
            RemoteUserId = userId,
            SocketId = SocketId,
            Channel = isServerHandled ? SERVER_CHANNEL : CLIENT_CHANNEL,
            AllowDelayedDelivery = false,
            Reliability = channel == NetworkChannel.Reliable 
                ? PacketReliability.ReliableUnordered 
                : PacketReliability.UnreliableUnordered,
            DisableAutoAcceptConnection = false
        };

        for (int i = 0; i < totalFragments; i++)
        {
            int offset = i * maxDataPerFragment;
            int fragmentSize = System.Math.Min(maxDataPerFragment, data.Length - offset);
            int packetSize = HEADER_SIZE + fragmentSize;

            byte[] packet = RentBuffer(packetSize);
            try
            {
                WriteFragmentHeader(packet, fragmentId, (ushort)i, (ushort)totalFragments);
                Array.Copy(data, offset, packet, HEADER_SIZE, fragmentSize);

                options.Data = new ArraySegment<byte>(packet, 0, packetSize);
                var result = EOSManager.P2PInterface.SendPacket(ref options);

                if (result != Result.Success)
                {
                    FusionLogger.Error($"Failed to send fragment {i}/{totalFragments}: {result}");
                    return result;
                }
            }
            finally
            {
                ReturnBuffer(packet);
            }
        }

        return Result.Success;
    }

    private static bool TryGetNextPacketSize(ref GetNextReceivedPacketSizeOptions options, out uint packetSize)
    {
        return EOSManager.P2PInterface.GetNextReceivedPacketSize(ref options, out packetSize) == Result.Success;
    }

    private static bool TryReceivePacket(ref ReceivePacketOptions options, uint packetSize, out ReceivedPacketData packetData)
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
        
        packetData = new ReceivedPacketData(buffer, (int)bytesWritten, peerId, channel == SERVER_CHANNEL);
        return true;
    }

    private static void ProcessReceivedPacket(ReceivedPacketData packetData)
    {
        ReadOnlySpan<byte> messageBuffer;

        if (IsFragment(packetData.Buffer))
        {
            if (!TryHandleFragment(packetData.Buffer, packetData.BytesWritten, packetData.PeerId, out byte[] reassembledData))
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
            if (!IsFragment(packetData.Buffer))
                ReturnBuffer(packetData.Buffer);
        }
    }

    private static bool TryHandleFragment(byte[] buffer, int bytesWritten, ProductUserId peerId, out byte[] reassembledData)
    {
        reassembledData = null;

        if (bytesWritten < HEADER_SIZE)
            return false;

        var (fragmentId, fragmentPart, totalFragments) = ReadFragmentHeader(buffer);

        if (totalFragments == 0 || totalFragments > MAX_FRAGMENTS || fragmentPart >= totalFragments)
            return false;

        var key = (peerId.ToString(), fragmentId);

        if (!_incomingFragments.TryGetValue(key, out var collection))
        {
            collection = new FragmentCollection
            {
                Fragments = new byte[totalFragments][],
                ReceivedFlags = new bool[totalFragments],
                ReceivedCount = 0,
                TotalSize = 0,
                LastReceived = DateTime.UtcNow
            };
            _incomingFragments[key] = collection;
        }

        if (collection.ReceivedFlags[fragmentPart])
            return false;

        int fragmentDataSize = bytesWritten - HEADER_SIZE;
        collection.Fragments[fragmentPart] = new byte[fragmentDataSize];
        Array.Copy(buffer, HEADER_SIZE, collection.Fragments[fragmentPart], 0, fragmentDataSize);
        collection.ReceivedFlags[fragmentPart] = true;
        collection.ReceivedCount++;
        collection.TotalSize += fragmentDataSize;
        collection.LastReceived = DateTime.UtcNow;
        _incomingFragments[key] = collection;

        if (collection.ReceivedCount == totalFragments)
        {
            reassembledData = new byte[collection.TotalSize];
            int offset = 0;

            for (int i = 0; i < collection.Fragments.Length; i++)
            {
                Array.Copy(collection.Fragments[i], 0, reassembledData, offset, collection.Fragments[i].Length);
                offset += collection.Fragments[i].Length;
            }

            _incomingFragments.Remove(key);
            return true;
        }

        return false;
    }

    private static bool IsFragment(byte[] buffer)
    {
        return buffer.Length >= 2 && BinaryPrimitives.ReadUInt16LittleEndian(buffer.AsSpan(0, 2)) == MAGIC_MARKER;
    }

    private static void WriteFragmentHeader(byte[] packet, ushort fragmentId, ushort fragmentIndex, ushort totalFragments)
    {
        var span = packet.AsSpan();
        BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(0, 2), MAGIC_MARKER);
        BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(2, 2), fragmentId);
        BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(4, 2), fragmentIndex);
        BinaryPrimitives.WriteUInt16LittleEndian(span.Slice(6, 2), totalFragments);
    }

    private static (ushort FragmentId, ushort FragmentPart, ushort TotalFragments) ReadFragmentHeader(byte[] buffer)
    {
        var span = buffer.AsSpan();
        return (
            BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(2, 2)),
            BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(4, 2)),
            BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(6, 2))
        );
    }

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