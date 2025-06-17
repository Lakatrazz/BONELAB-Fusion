using LabFusion.Extensions;
using LabFusion.Math;

using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace LabFusion.Network.Serialization;

public sealed class NetWriter : INetSerializer, IDisposable
{
    public const int DefaultCapacity = 4096;

    public bool IsReader => false;

    public int Position { get; set; }

    public int Length => Position;

    private byte[] _buffer = null;

    public ArraySegment<byte> Buffer => new(_buffer, 0, Length);

    /// <summary>
    /// Creates a new NetWriter with a capacity of <see cref="DefaultCapacity"/>.
    /// </summary>
    /// <returns></returns>
    public static NetWriter Create() => Create(DefaultCapacity);

    /// <summary>
    /// Creates a new NetWriter with a set capacity if not null, or <see cref="DefaultCapacity"/> if it is null.
    /// </summary>
    /// <param name="capacity">The maximum amount of bytes that can be written into this writer.</param>
    /// <returns></returns>
    public static NetWriter Create(int? capacity) => Create(capacity ?? DefaultCapacity);

    /// <summary>
    /// Creates a new NetWriter with a set capacity.
    /// </summary>
    /// <param name="capacity">The maximum amount of bytes that can be written into this writer.</param>
    /// <returns></returns>
    public static NetWriter Create(int capacity)
    {
        return new NetWriter
        {
            _buffer = ArrayPool<byte>.Shared.Rent(capacity),
            Position = 0,
        };
    }

    public void Write(byte value)
    {
        _buffer[Position++] = value;
    }

    public void Write(bool value)
    {
        Write((byte)(value ? 1 : 0));
    }

    public void Write(double value)
    {
        BinaryPrimitives.WriteDoubleBigEndian(new Span<byte>(_buffer, Position, sizeof(double)), value);
        Position += sizeof(double);
    }

    public void Write(short value)
    {
        BinaryPrimitives.WriteInt16BigEndian(new Span<byte>(_buffer, Position, sizeof(short)), value);
        Position += sizeof(short);
    }

    public void Write(int value)
    {
        BinaryPrimitives.WriteInt32BigEndian(new Span<byte>(_buffer, Position, sizeof(int)), value);
        Position += sizeof(int);
    }

    public void Write(long value)
    {
        BinaryPrimitives.WriteInt64BigEndian(new Span<byte>(_buffer, Position, sizeof(long)), value);
        Position += sizeof(long);
    }

    public void Write(sbyte value)
    {
        Write(value.ToByte());
    }

    public void Write(float value)
    {
        BinaryPrimitives.WriteSingleBigEndian(new Span<byte>(_buffer, Position, sizeof(float)), value);
        Position += sizeof(float);
    }

    public void Write(ushort value)
    {
        BinaryPrimitives.WriteUInt16BigEndian(new Span<byte>(_buffer, Position, sizeof(ushort)), value);
        Position += sizeof(ushort);
    }

    public void Write(uint value)
    {
        BinaryPrimitives.WriteUInt32BigEndian(new Span<byte>(_buffer, Position, sizeof(uint)), value);
        Position += sizeof(uint);
    }

    public void Write(ulong value)
    {
        BinaryPrimitives.WriteUInt64BigEndian(new Span<byte>(_buffer, Position, sizeof(ulong)), value);
        Position += sizeof(ulong);
    }

    public void Write(string value)
    {
        Write(value, Encoding.UTF8);
    }

    public void Write(string value, Encoding encoding)
    {
        if (value == null)
        {
            Write(-1);
            return;
        }

        int byteCount = encoding.GetByteCount(value);
        Write(byteCount);

        encoding.GetBytes(value, 0, value.Length, _buffer, Position);
        Position += byteCount;
    }

    public void Write(byte[] value)
    {
        Write(value.Length);

        System.Buffer.BlockCopy(value, 0, _buffer, Position, value.Length);
        Position += value.Length;
    }

    public void Write(ArraySegment<byte> value)
    {
        Write(value.Count);

        for (var i = 0; i < value.Count; i++)
        {
            _buffer[Position] = value[i];
            Position++;
        }
    }

    public void Write(string[] value)
    {
        Write(value.Length);

        for (var i = 0; i < value.Length; i++)
        {
            Write(value[i]);
        }
    }

    public void Write<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        Write(EnumConverter.ConvertToInt32(value));
    }

    public void Write<TEnum>(TEnum value, Precision precision) where TEnum : struct, Enum
    {
        switch (precision)
        {
            default:
                Write(EnumConverter.ConvertToInt32(value));
                break;
            case Precision.TwoBytes:
                Write(EnumConverter.ConvertToInt16(value));
                break;
            case Precision.OneByte:
                Write(EnumConverter.ConvertToByte(value));
                break;
        }
    }

    public void Write(byte? value)
    {
        Write(value.HasValue);

        if (value.HasValue)
        {
            Write(value.Value);
        }
    }

    public void Write(ushort? value)
    {
        Write(value.HasValue);

        if (value.HasValue)
        {
            Write(value.Value);
        }
    }

    public void Write(int? value)
    {
        Write(value.HasValue);

        if (value.HasValue)
        {
            Write(value.Value);
        }
    }

    public void SerializeValue(ref byte value) => Write(value);

    public void SerializeValue(ref bool value) => Write(value);

    public void SerializeValue(ref int value) => Write(value);

    public void SerializeValue(ref double value) => Write(value);

    public void SerializeValue(ref short value) => Write(value);

    public void SerializeValue(ref long value) => Write(value);

    public void SerializeValue(ref sbyte value) => Write(value);

    public void SerializeValue(ref float value) => Write(value);

    public void SerializeValue(ref ushort value) => Write(value);

    public void SerializeValue(ref uint value) => Write(value);

    public void SerializeValue(ref ulong value) => Write(value);

    public void SerializeValue(ref string value) => Write(value);

    public void SerializeValue(ref byte[] value) => Write(value);

    public void SerializeValue(ref ArraySegment<byte> value) => Write(value);

    public void SerializeValue(ref string[] value) => Write(value);

    public void SerializeValue<TEnum>(ref TEnum value) where TEnum : struct, Enum => Write(value);

    public void SerializeValue<TEnum>(ref TEnum value, Precision precision) where TEnum : struct, Enum => Write(value, precision);

    public void SerializeValue(ref byte? value) => Write(value);

    public void SerializeValue(ref ushort? value) => Write(value);

    public void SerializeValue(ref int? value) => Write(value);

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        ArrayPool<byte>.Shared.Return(_buffer);
    }
}
