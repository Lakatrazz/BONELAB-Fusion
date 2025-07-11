using Epic.OnlineServices;

using LabFusion.Utilities;

namespace LabFusion.Network;

// This shit sucks
internal static class PacketFragmentation
{
    private struct FragmentCollection
    {
        public byte[][] Fragments;
        public int ReceivedCount;
        public int TotalSize;
        public DateTime LastReceived;
    }

    private const int HEADER_SIZE = 8;
    private const ushort MAGIC_MARKER = 0xF2A9;
    private const int MAX_FRAGMENTS = 1000;
    private const int FRAGMENT_CLEANUP_SECONDS = 30;

    private static readonly Dictionary<(string, ushort), FragmentCollection> _incomingFragments = new();

    internal static unsafe Result SendFragmentedPacket(ProductUserId userId, byte[] data, NetworkChannel channel, bool isServerHandled, int maxPacketSize)
    {
        int maxDataPerFragment = maxPacketSize - HEADER_SIZE;
        int totalFragments = (data.Length + maxDataPerFragment - 1) / maxDataPerFragment;

        if (totalFragments > MAX_FRAGMENTS)
        {
            FusionLogger.Error($"Message too large: {data.Length} bytes would create {totalFragments} fragments");
            return Result.InvalidParameters;
        }

        ushort fragmentId = (ushort)Random.Shared.Next(ushort.MaxValue);
        var reliability = channel == NetworkChannel.Reliable
            ? Epic.OnlineServices.P2P.PacketReliability.ReliableUnordered
            : Epic.OnlineServices.P2P.PacketReliability.UnreliableUnordered;

        byte channelByte = isServerHandled ? (byte)2 : (byte)1;

        fixed (byte* dataPtr = data)
        {
            for (int i = 0; i < totalFragments; i++)
            {
                int offset = i * maxDataPerFragment;
                int fragmentSize = System.Math.Min(maxDataPerFragment, data.Length - offset);

                byte[] fragmentPacket = new byte[HEADER_SIZE + fragmentSize];

                fixed (byte* packetPtr = fragmentPacket)
                {
                    WriteFragmentHeader(packetPtr, fragmentId, (ushort)i, (ushort)totalFragments);
                    Buffer.MemoryCopy(dataPtr + offset, packetPtr + HEADER_SIZE, fragmentSize, fragmentSize);
                }

                var sendOptions = new Epic.OnlineServices.P2P.SendPacketOptions()
                {
                    LocalUserId = EOSNetworkLayer.LocalUserId,
                    RemoteUserId = userId,
                    SocketId = EOSSocketHandler.SocketId,
                    Channel = channelByte,
                    Data = new ArraySegment<byte>(fragmentPacket),
                    AllowDelayedDelivery = false,
                    Reliability = reliability,
                    DisableAutoAcceptConnection = false,
                };

                var result = EOSManager.P2PInterface.SendPacket(ref sendOptions);
                if (result != Result.Success)
                {
                    FusionLogger.Error($"Failed to send fragment {i}/{totalFragments}: {result}");
                    return result;
                }
            }
        }

        return Result.Success;
    }

    internal static unsafe bool TryHandleFragment(byte[] buffer, int bytesWritten, ProductUserId peerId, out byte[] reassembledData)
    {
        reassembledData = null;

        if (bytesWritten < HEADER_SIZE)
            return false;

        if (!IsFragment(buffer))
            return false;

        var header = ReadFragmentHeader(buffer);
        if (!ValidateFragmentHeader(header, bytesWritten))
            return false;

        var key = (peerId.ToString(), header.FragmentId);

        if (!_incomingFragments.TryGetValue(key, out var collection))
        {
            collection = CreateNewFragmentCollection(header.TotalFragments);
            _incomingFragments[key] = collection;
        }

        if (!ValidateFragmentCollection(collection, header, key))
            return false;

        if (TryAddFragment(ref collection, header, buffer, bytesWritten, key))
        {
            _incomingFragments[key] = collection;

            if (collection.ReceivedCount == header.TotalFragments)
            {
                return TryReassembleFragments(collection, key, out reassembledData);
            }
        }

        return false;
    }

    internal static unsafe bool IsFragment(byte[] buffer)
    {
        if (buffer.Length < 2) return false;

        fixed (byte* bufferPtr = buffer)
        {
            return *(ushort*)bufferPtr == MAGIC_MARKER;
        }
    }

    internal static void CleanupOldFragments()
    {
        var cutoffTime = DateTime.UtcNow.AddSeconds(-FRAGMENT_CLEANUP_SECONDS);
        var keysToRemove = new List<(string, ushort)>();

        foreach (var kvp in _incomingFragments)
        {
            if (kvp.Value.LastReceived < cutoffTime)
            {
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _incomingFragments.Remove(key);
        }
    }

    private static unsafe void WriteFragmentHeader(byte* packetPtr, ushort fragmentId, ushort fragmentIndex, ushort totalFragments)
    {
        *(ushort*)packetPtr = MAGIC_MARKER;                    // Magic marker
        *(ushort*)(packetPtr + 2) = fragmentId;                // Fragment ID
        *(ushort*)(packetPtr + 4) = fragmentIndex;             // Fragment index
        *(ushort*)(packetPtr + 6) = totalFragments;            // Total fragments
    }

    private static unsafe (ushort FragmentId, ushort FragmentPart, ushort TotalFragments) ReadFragmentHeader(byte[] buffer)
    {
        fixed (byte* bufferPtr = buffer)
        {
            return (
                *(ushort*)(bufferPtr + 2),  // Fragment ID
                *(ushort*)(bufferPtr + 4),  // Fragment part
                *(ushort*)(bufferPtr + 6)   // Total fragments
            );
        }
    }

    private static bool ValidateFragmentHeader((ushort FragmentId, ushort FragmentPart, ushort TotalFragments) header, int bytesWritten)
    {
        if (header.TotalFragments == 0 || header.TotalFragments > MAX_FRAGMENTS)
        {
            FusionLogger.Error($"Invalid totalFragments: {header.TotalFragments}");
            return false;
        }

        if (header.FragmentPart >= header.TotalFragments)
        {
            FusionLogger.Error($"Fragment part {header.FragmentPart} >= total fragments {header.TotalFragments}");
            return false;
        }

        if (bytesWritten < HEADER_SIZE)
        {
            FusionLogger.Error($"Invalid fragment data size: {bytesWritten - HEADER_SIZE}");
            return false;
        }

        return true;
    }

    private static FragmentCollection CreateNewFragmentCollection(ushort totalFragments)
    {
        return new FragmentCollection
        {
            Fragments = new byte[totalFragments][],
            ReceivedCount = 0,
            TotalSize = 0,
            LastReceived = DateTime.UtcNow
        };
    }

    private static bool ValidateFragmentCollection(FragmentCollection collection, (ushort FragmentId, ushort FragmentPart, ushort TotalFragments) header, (string, ushort) key)
    {
        if (collection.Fragments.Length != header.TotalFragments)
        {
            FusionLogger.Error($"Fragment collection length mismatch: {collection.Fragments.Length} != {header.TotalFragments}");
            _incomingFragments.Remove(key);
            return false;
        }
        return true;
    }

    private static unsafe bool TryAddFragment(ref FragmentCollection collection, (ushort FragmentId, ushort FragmentPart, ushort TotalFragments) header, byte[] buffer, int bytesWritten, (string, ushort) key)
    {
        if (collection.Fragments[header.FragmentPart] != null)
            return true;

        int fragmentDataSize = bytesWritten - HEADER_SIZE;
        var fragmentData = new byte[fragmentDataSize];

        fixed (byte* srcPtr = buffer, destPtr = fragmentData)
        {
            Buffer.MemoryCopy(srcPtr + HEADER_SIZE, destPtr, fragmentDataSize, fragmentDataSize);
        }

        collection.Fragments[header.FragmentPart] = fragmentData;
        collection.ReceivedCount++;
        collection.TotalSize += fragmentDataSize;
        collection.LastReceived = DateTime.UtcNow;

        return true;
    }

    private static unsafe bool TryReassembleFragments(FragmentCollection collection, (string, ushort) key, out byte[] reassembledData)
    {
        reassembledData = null;

        try
        {
            reassembledData = new byte[collection.TotalSize];
            int offset = 0;

            fixed (byte* destPtr = reassembledData)
            {
                for (int i = 0; i < collection.Fragments.Length; i++)
                {
                    var fragment = collection.Fragments[i];
                    if (fragment == null)
                    {
                        FusionLogger.Error($"Missing fragment {i} during reassembly");
                        _incomingFragments.Remove(key);
                        return false;
                    }

                    if (offset + fragment.Length > reassembledData.Length)
                    {
                        FusionLogger.Error($"Fragment reassembly would overflow: {offset + fragment.Length} > {reassembledData.Length}");
                        _incomingFragments.Remove(key);
                        return false;
                    }

                    fixed (byte* srcPtr = fragment)
                    {
                        Buffer.MemoryCopy(srcPtr, destPtr + offset, fragment.Length, fragment.Length);
                    }
                    offset += fragment.Length;
                }
            }

            _incomingFragments.Remove(key);
            return true;
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("Error during fragment reassembly", ex);
            _incomingFragments.Remove(key);
            return false;
        }
    }
}
