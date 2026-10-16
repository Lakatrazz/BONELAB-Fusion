using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

// Edited but not original
// https://github.com/TrentSterling/fishnet-eos-native/blob/master/Assets/FishNet.Transport.EOSNative/PacketFragmenter.cs

namespace MarrowFusion.Epic;

internal class PacketFragmenter
{
    internal const int HeaderSize = sizeof(uint) + sizeof(ushort) + sizeof(byte);
    internal const int MaxPacketSize = P2PInterface.MAX_PACKET_SIZE;
    internal const int MaxPayloadSize = MaxPacketSize - HeaderSize;
    
    internal const int MaxFragmentsPerMessage = 64;
    internal const int MaxConcurrentReassemblies = 32;
    
    internal const int SlotTimeoutMs = 10000;

    private uint _nextPacketId;

    private readonly byte[] _sendBuffer = new byte[MaxPacketSize];

    private readonly ReassemblySlot[] _slots;

    internal PacketFragmenter()
    {
        _slots = new ReassemblySlot[MaxConcurrentReassemblies];
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].Buffer = new byte[MaxFragmentsPerMessage * MaxPayloadSize];
            _slots[i].InUse = false;
        }
    }

    internal FragmentEnumerator Fragment(ArraySegment<byte> data)
    {
        return new FragmentEnumerator(this, data, _nextPacketId++);
    }

    internal struct FragmentEnumerator
    {
        private readonly PacketFragmenter _fragmenter;
        private readonly ArraySegment<byte> _data;
        private readonly uint _packetId;
        private readonly int _totalFragments;
        private int _index;

        internal FragmentEnumerator(PacketFragmenter fragmenter, ArraySegment<byte> data, uint packetId)
        {
            _fragmenter = fragmenter;
            _data = data;
            _packetId = packetId;
            _totalFragments = data.Count == 0 ? 1 : (data.Count + MaxPayloadSize - 1) / MaxPayloadSize;
            _index = -1;
            Current = default;
        }

        public FragmentEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            _index++;
            if (_index >= _totalFragments)
            {
                return false;
            }

            var buffer = _fragmenter._sendBuffer;

            if (_data.Count == 0)
            {
                WriteHeader(buffer, _packetId, 0, true);
                Current = new ArraySegment<byte>(buffer, 0, HeaderSize);
                return true;
            }

            int offset = _index * MaxPayloadSize;
            int remaining = _data.Count - offset;
            int payloadLength = Math.Min(MaxPayloadSize, remaining);
            bool isLast = _index == _totalFragments - 1;

            WriteHeader(buffer, _packetId, (ushort)_index, isLast);
            Array.Copy(_data.Array, _data.Offset + offset, buffer, HeaderSize, payloadLength);

            Current = new ArraySegment<byte>(buffer, 0, HeaderSize + payloadLength);
            return true;
        }

        public ArraySegment<byte> Current { get; private set; }
    }

    internal static bool NeedsFragmentation(int dataLength)
    {
        return dataLength > MaxPayloadSize;
    }

    private static void WriteHeader(byte[] buffer, uint packetId, ushort fragmentId, bool lastFragment)
    {
        buffer[0] = (byte)packetId;
        buffer[1] = (byte)(packetId >> 8);
        buffer[2] = (byte)(packetId >> 16);
        buffer[3] = (byte)(packetId >> 24);
        
        buffer[4] = (byte)fragmentId;
        buffer[5] = (byte)(fragmentId >> 8);
        
        buffer[6] = lastFragment ? (byte)1 : (byte)0;
    }

    internal bool ProcessIncoming(ProductUserId senderId, ArraySegment<byte> data, byte channel, out ArraySegment<byte> payload)
    {
        payload = default;

        if (data.Count < HeaderSize)
        {
            EpicModule.Logger.Warn($"Received packet too small for header: {data.Count} bytes (need {HeaderSize})");
            return false;
        }

        uint packetId = BitConverter.ToUInt32(data.Array, data.Offset);
        ushort fragmentId = BitConverter.ToUInt16(data.Array, data.Offset + 4);
        bool lastFragment = data.Array[data.Offset + 6] == 1;

        int payloadLength = data.Count - HeaderSize;
        
        if (fragmentId == 0 && lastFragment)
        {
            payload = new ArraySegment<byte>(data.Array, data.Offset + HeaderSize, payloadLength);
            return true;
        }

        if (fragmentId >= MaxFragmentsPerMessage)
        {
            EpicModule.Logger.Warn($"Fragment id {fragmentId} exceeds max supported fragments per message ({MaxFragmentsPerMessage}); dropping packet");
            return false;
        }

        int slotIndex = FindOrCreateSlot(senderId, packetId, channel);
        if (slotIndex < 0)
        {
            EpicModule.Logger.Warn("No free reassembly slot available. Dropping fragment");
            return false;
        }

        ref var slot = ref _slots[slotIndex];
        slot.LastActivityTicks = Environment.TickCount;

        int offset = fragmentId * MaxPayloadSize;
        if (offset + payloadLength > slot.Buffer.Length)
        {
            EpicModule.Logger.Warn("Fragment payload exceeds reassembly buffer bounds. Dropping reassembly");
            FreeSlot(ref slot);
            return false;
        }

        Array.Copy(data.Array, data.Offset + HeaderSize, slot.Buffer, offset, payloadLength);
        slot.ReceivedMask |= 1UL << fragmentId;

        if (lastFragment)
        {
            slot.ExpectedCount = fragmentId + 1;
            slot.LastFragmentLength = payloadLength;
            
            ulong invalidMask = slot.ExpectedCount >= 64 ? 0UL : ~((1UL << slot.ExpectedCount) - 1UL);
            if ((slot.ReceivedMask & invalidMask) != 0)
            {
                EpicModule.Logger.Warn("Received fragment id beyond declared message length. Dropping reassembly.");
                FreeSlot(ref slot);
                return false;
            }
        }

        if (slot.ExpectedCount == -1)
        {
            return false;
        }

        ulong completeMask = slot.ExpectedCount >= 64 ? ulong.MaxValue : (1UL << slot.ExpectedCount) - 1UL;
        if ((slot.ReceivedMask & completeMask) != completeMask)
        {
            return false;
        }

        int totalSize = (slot.ExpectedCount - 1) * MaxPayloadSize + slot.LastFragmentLength;
        payload = new ArraySegment<byte>(slot.Buffer, 0, totalSize);
        
        FreeSlot(ref slot);
        return true;
    }

    private int FindOrCreateSlot(ProductUserId senderId, uint packetId, byte channel)
    {
        int freeIndex = -1;
        int staleIndex = -1;
        int staleAge = -1;

        for (int i = 0; i < _slots.Length; i++)
        {
            ref var slot = ref _slots[i];

            if (slot.InUse)
            {
                if (slot.PacketId == packetId && slot.Channel == channel && Equals(slot.SenderId, senderId))
                {
                    return i;
                }

                int age = unchecked(Environment.TickCount - slot.LastActivityTicks);
                if (age > SlotTimeoutMs && age > staleAge)
                {
                    staleIndex = i;
                    staleAge = age;
                }
            }
            else if (freeIndex == -1)
            {
                freeIndex = i;
            }
        }

        if (freeIndex != -1)
        {
            InitSlot(freeIndex, senderId, packetId, channel);
            return freeIndex;
        }

        if (staleIndex != -1)
        {
            EpicModule.Logger.Warn($"Reassembly slot pool full. Evicting stale slot (idle {staleAge}ms) to make room");
            InitSlot(staleIndex, senderId, packetId, channel);
            return staleIndex;
        }

        return -1;
    }

    private void InitSlot(int index, ProductUserId senderId, uint packetId, byte channel)
    {
        ref var slot = ref _slots[index];
        slot.InUse = true;
        slot.SenderId = senderId;
        slot.PacketId = packetId;
        slot.Channel = channel;
        slot.ExpectedCount = -1;
        slot.ReceivedMask = 0UL;
        slot.LastFragmentLength = 0;
        slot.LastActivityTicks = Environment.TickCount;
    }

    private static void FreeSlot(ref ReassemblySlot slot)
    {
        slot.InUse = false;
        slot.SenderId = null;
        slot.ExpectedCount = -1;
        slot.ReceivedMask = 0UL;
        slot.LastFragmentLength = 0;
    }

    internal void ClearPendingForSender(ProductUserId senderId)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            ref var slot = ref _slots[i];
            if (slot.InUse && Equals(slot.SenderId, senderId))
            {
                FreeSlot(ref slot);
            }
        }
    }

    internal void ClearAll()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            FreeSlot(ref _slots[i]);
        }
    }

    private struct ReassemblySlot
    {
        internal bool InUse;
        internal ProductUserId SenderId;
        internal uint PacketId;
        internal byte Channel;
        
        internal int ExpectedCount;
        internal ulong ReceivedMask;
        internal int LastFragmentLength;
        internal int LastActivityTicks;
        
        internal byte[] Buffer;
    }
}