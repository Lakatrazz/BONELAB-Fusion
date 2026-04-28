using System.Buffers.Binary;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Handles fragment header serialization/deserialization.
/// </summary>
internal static class FragmentHeader
{
    internal const int Size = 8;
    private const ushort MagicMarker = 0xF2A9;

    internal static void Write(Span<byte> buffer, ushort fragmentId, ushort fragmentIndex, ushort totalFragments)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(buffer[..2], MagicMarker);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(2, 2), fragmentId);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(4, 2), fragmentIndex);
        BinaryPrimitives.WriteUInt16LittleEndian(buffer.Slice(6, 2), totalFragments);
    }

    internal static (ushort FragmentId, ushort FragmentIndex, ushort TotalFragments) Read(ReadOnlySpan<byte> buffer)
    {
        return (
            BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(2, 2)),
            BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(4, 2)),
            BinaryPrimitives.ReadUInt16LittleEndian(buffer.Slice(6, 2))
        );
    }

    internal static bool IsFragment(ReadOnlySpan<byte> buffer)
    {
        return buffer.Length >= Size && BinaryPrimitives.ReadUInt16LittleEndian(buffer[..2]) == MagicMarker;
    }
}