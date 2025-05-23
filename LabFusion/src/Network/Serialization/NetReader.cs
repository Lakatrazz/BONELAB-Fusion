using LabFusion.Extensions;
using LabFusion.Utilities;

using System.Runtime.CompilerServices;
using System.Text;

namespace LabFusion.Network.Serialization;

public sealed class NetReader : INetSerializer, IDisposable
{
    private byte[] _buffer = null;

    public bool IsReader => true;

    public int Position { get; set; }

    public int Length { get; set; }

    public static NetReader Create(byte[] buffer)
    {
        var reader = new NetReader
        {
            _buffer = buffer,
            Position = 0,
            Length = buffer.Length,
        };
        return reader;
    }

    public byte ReadByte()
    {
        if (Position >= Length)
        {
            throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 1 byte but reader only has {0} bytes remaining.", Length - Position));
        }

        return _buffer[Position++];
    }

    public bool ReadBoolean()
    {
        return ReadByte() == 1;
    }

    public double ReadDouble()
    {
        if (Position + sizeof(double) > Length)
        {
            throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 8 bytes but reader only has {0} bytes remaining.", Length - Position));
        }

        double result = BigEndianHelper.ReadDouble(_buffer, Position);
        Position += sizeof(double);

        return result;
    }

    public short ReadInt16()
    {
        if (Position + sizeof(short) > Length)
        {
            throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 2 bytes but reader only has {0} bytes remaining.", Length - Position));
        }
        short result = BigEndianHelper.ReadInt16(_buffer, Position);
        Position += sizeof(short);
        return result;
    }

    public int ReadInt32()
    {
        if (Position + sizeof(int) > Length)
        {
            throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 4 bytes but reader only has {0} bytes remaining.", Length - Position));
        }
        int result = BigEndianHelper.ReadInt32(_buffer, Position);
        Position += sizeof(int);
        return result;
    }

    public long ReadInt64()
    {
        if (Position + sizeof(long) > Length)
        {
            throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 8 bytes but reader only has {0} bytes remaining.", Length - Position));
        }
        long result = BigEndianHelper.ReadInt64(_buffer, Position);
        Position += sizeof(long);
        return result;
    }

    public sbyte ReadSByte()
    {
        return ReadByte().ToSByte();
    }

    public float ReadSingle()
    {
        if (Position + sizeof(float) > Length)
        {
            throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 4 bytes but reader only has {0} bytes remaining.", Length - Position));
        }
        float result = BigEndianHelper.ReadSingle(_buffer, Position);
        Position += sizeof(float);
        return result;
    }

    public string ReadString()
    {
        return ReadString(Encoding.UTF8);
    }

    public string ReadString(Encoding encoding)
    {
        int length = ReadInt32();

        if (length <= -1)
        {
            return null;
        }

        if (Position + length > Length)
        {
            throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", length, Length - Position - 4));
        }

        string value = encoding.GetString(_buffer, Position, length);
        Position += length;

        return value;
    }

    public ushort ReadUInt16()
    {
        if (Position + sizeof(ushort) > Length)
        {
            throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 2 bytes but reader only has {0} bytes remaining.", Length - Position));
        }

        ushort result = BigEndianHelper.ReadUInt16(_buffer, Position);
        Position += sizeof(ushort);

        return result;
    }

    public uint ReadUInt32()
    {
        if (Position + sizeof(uint) > Length)
        {
            throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 4 bytes but reader only has {0} bytes remaining.", Length - Position));
        }

        uint result = BigEndianHelper.ReadUInt32(_buffer, Position);
        Position += sizeof(uint);

        return result;
    }

    public ulong ReadUInt64()
    {
        if (Position + sizeof(ulong) > Length)
        {
            throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 8 bytes but reader only has {0} bytes remaining.", Length - Position));
        }

        ulong result = BigEndianHelper.ReadUInt64(_buffer, Position);
        Position += sizeof(ulong);

        return result;
    }

    public byte[] ReadBytes()
    {
        int length = ReadInt32();

        if (Position + length > Length)
        {
            throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", length, Length - Position - 4));
        }

        var array = new byte[length];

        Buffer.BlockCopy(_buffer, Position, array, 0, length);
        Position += length;

        return array;
    }

    public string[] ReadStrings()
    {
        int length = ReadInt32();

        string[] array = new string[length];

        for (int i = 0; i < length; i++)
        {
            array[i] = ReadString();
        }
        return array;
    }

    public TEnum ReadEnum<TEnum>() where TEnum : Enum
    {
        var value = ReadInt32();

        return Unsafe.As<int, TEnum>(ref value);
    }

    public TEnum ReadEnum<TEnum>(Precision precision) where TEnum : Enum
    {
        switch (precision)
        {
            default:
                var full = ReadInt32();
                return Unsafe.As<int, TEnum>(ref full);
            case Precision.TwoBytes:
                var twoBytes = ReadInt16();
                return Unsafe.As<short, TEnum>(ref twoBytes);
            case Precision.OneByte:
                var oneByte = ReadByte();
                return Unsafe.As<byte, TEnum>(ref oneByte);
        }
    }

    public byte? ReadByteNullable()
    {
        var hasValue = ReadBoolean();

        if (hasValue)
        {
            return ReadByte();
        }

        return null;
    }

    public ushort? ReadUInt16Nullable()
    {
        var hasValue = ReadBoolean();

        if (hasValue)
        {
            return ReadUInt16();
        }

        return null;
    }

    public int? ReadInt32Nullable()
    {
        var hasValue = ReadBoolean();

        if (hasValue)
        {
            return ReadInt32();
        }

        return null;
    }

    public void SerializeValue(ref byte value) => value = ReadByte();

    public void SerializeValue(ref bool value) => value = ReadBoolean();

    public void SerializeValue(ref int value) => value = ReadInt32();

    public void SerializeValue(ref double value) => value = ReadDouble();

    public void SerializeValue(ref short value) => value = ReadInt16();

    public void SerializeValue(ref long value) => value = ReadInt64();

    public void SerializeValue(ref sbyte value) => value = ReadSByte();

    public void SerializeValue(ref float value) => value = ReadSingle();

    public void SerializeValue(ref ushort value) => value = ReadUInt16();

    public void SerializeValue(ref uint value) => value = ReadUInt32();

    public void SerializeValue(ref ulong value) => value = ReadUInt64();

    public void SerializeValue(ref string value) => value = ReadString();

    public void SerializeValue(ref byte[] value) => value = ReadBytes();

    public void SerializeValue(ref string[] value) => value = ReadStrings();

    public void SerializeValue<TEnum>(ref TEnum value) where TEnum : Enum => value = ReadEnum<TEnum>();

    public void SerializeValue<TEnum>(ref TEnum value, Precision precision) where TEnum : Enum => value = ReadEnum<TEnum>(precision);

    public void SerializeValue(ref byte? value) => value = ReadByteNullable();

    public void SerializeValue(ref ushort? value) => value = ReadUInt16Nullable();

    public void SerializeValue(ref int? value) => value = ReadInt32Nullable();

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
