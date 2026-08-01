using System.Buffers;
using System.Buffers.Binary;
using Epic.OnlineServices.P2P;

namespace LabFusion.Network;

internal static class FragmentHeader
{
    internal const byte KindSingle = 0;
    internal const byte KindFragment = 1;
    internal const int KindPrefixSize = 1;
    
    internal const int HeaderSize = KindPrefixSize + 2 + 2 + 2 + 4;

    internal static void Write(Span<byte> buffer, ushort fragmentId, ushort fragmentIndex, ushort totalFragments, int totalLength)
    {
        buffer[0] = KindFragment;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(1, 2), fragmentId);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(3, 2), fragmentIndex);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(5, 2), totalFragments);
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(7, 4), totalLength);
    }

    internal static (ushort FragmentId, ushort FragmentIndex, ushort TotalFragments, int TotalLength) Read(ReadOnlySpan<byte> buffer)
    {
        return 
        (
            BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(1, 2)),
            BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(3, 2)),
            BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(5, 2)),
            BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(7, 4))
        );
    }

    internal static bool IsFragment(ReadOnlySpan<byte> buffer)
    {
        return buffer.Length >= KindPrefixSize && buffer[0] == KindFragment;
    }
}

internal class FragmentAssembler
{
    private struct ActiveAssembly
    {
        public byte[] Buffer;
        public int TotalLength;
        public int TotalFragments;
        public int ReceivedCount;
        public bool[] ReceivedFlags;
        public long ExpiryTicks;
    }

    private const int MaxPacketSize = P2PInterface.MAX_PACKET_SIZE;
    private const int MaxDataPerFragment = MaxPacketSize - FragmentHeader.HeaderSize;

    private readonly Dictionary<(string SenderId, ushort FragmentId), ActiveAssembly> _assemblies = new();
    private readonly object _lock = new();
    
    private long _lastCleanupTicks = DateTime.UtcNow.Ticks;
    private const long ExpiryDurationTicks = 10 * 10_000_000L;
    private const long CleanupIntervalTicks = 15 * 10_000_000L;

    private EOSBufferPool _bufferPool;
    
    internal FragmentAssembler(EOSBufferPool bufferPool)
    {
        _bufferPool = bufferPool;
    }

    internal bool TryAssemble(string senderId, ReadOnlySpan<byte> packetData, out byte[] completedBuffer, out int completedSize)
    {
        completedBuffer = null;
        completedSize = 0;

        if (packetData.Length < FragmentHeader.HeaderSize)
            return false;

        var (fragmentId, index, total, totalLength) = FragmentHeader.Read(packetData);

        if (total == 0 || index >= total || totalLength <= 0)
            return false;

        var key = (senderId, fragmentId);
        int payloadOffset = FragmentHeader.HeaderSize;
        int payloadLength = packetData.Length - payloadOffset;

        lock (_lock)
        {
            if (!_assemblies.TryGetValue(key, out var assembly))
            {
                assembly = new ActiveAssembly
                {
                    Buffer = _bufferPool.Rent(totalLength),
                    TotalLength = totalLength,
                    TotalFragments = total,
                    ReceivedCount = 0,
                    ReceivedFlags = new bool[total],
                    ExpiryTicks = DateTime.UtcNow.Ticks + ExpiryDurationTicks
                };
            }

            if (assembly.ReceivedFlags[index])
                return false;
            
            int writeOffset = index * MaxDataPerFragment;
            
            if (writeOffset < 0 || writeOffset + payloadLength > assembly.TotalLength)
            {
                _bufferPool.Return(assembly.Buffer);
                _assemblies.Remove(key);
                return false;
            }
            
            packetData.Slice(payloadOffset, payloadLength).CopyTo(assembly.Buffer.AsSpan(writeOffset));
            assembly.ReceivedFlags[index] = true;
            assembly.ReceivedCount++;
            assembly.ExpiryTicks = DateTime.UtcNow.Ticks + ExpiryDurationTicks;

            if (assembly.ReceivedCount == assembly.TotalFragments)
            {
                completedBuffer = assembly.Buffer;
                completedSize = assembly.TotalLength;
                _assemblies.Remove(key);
                return true;
            }
            
            _assemblies[key] = assembly;
            return false;
        }
    }

    internal void CleanupIfNeeded()
    {
        long now = DateTime.UtcNow.Ticks;
        if (now - _lastCleanupTicks < CleanupIntervalTicks)
            return;

        lock (_lock)
        {
            _lastCleanupTicks = now;
            var staleKeys = new List<(string SenderId, ushort FragmentId)>();
            
            foreach (var kvp in _assemblies)
            {
                if (now > kvp.Value.ExpiryTicks)
                {
                    staleKeys.Add(kvp.Key);
                }
            }

            foreach (var key in staleKeys)
            {
                if (_assemblies.TryGetValue(key, out var assembly))
                {
                    _bufferPool.Return(assembly.Buffer);
                    _assemblies.Remove(key);
                }
            }
        }
    }
}