using System.Buffers.Binary;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Handles fragment header serialization/deserialization.
/// </summary>
internal static class FragmentHeader
{
    internal const byte KindSingle = 0;
    internal const byte KindFragment = 1;
    
    internal const int KindPrefixSize = 1;
    
    internal const int Size = KindPrefixSize + 6;

    internal static void Write(Span<byte> buffer, ushort fragmentId, ushort fragmentIndex, ushort totalFragments)
    {
        buffer[0] = KindFragment;
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(1, 2), fragmentId);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(3, 2), fragmentIndex);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(5, 2), totalFragments);
    }

    internal static (ushort FragmentId, ushort FragmentIndex, ushort TotalFragments) Read(ReadOnlySpan<byte> buffer)
    {
        return (
            BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(1, 2)),
            BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(3, 2)),
            BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(5, 2))
        );
    }

    internal static bool IsFragment(ReadOnlySpan<byte> buffer)
    {
        return buffer.Length >= KindPrefixSize && buffer[0] == KindFragment;
    }
}