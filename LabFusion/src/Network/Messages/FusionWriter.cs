using System.Buffers;
using System.Text;

using LabFusion.Extensions;
using LabFusion.Utilities;
using LabFusion.Data;

using UnityEngine;

namespace LabFusion.Network;

using System;

public class FusionWriter : IDisposable
{
    public const int MaxSize = 4096;

    public int Position { get; set; }

    public int Length
    {
        get
        {
            return Position;
        }
    }

    private byte[] _buffer;

    public ArraySegment<byte> Buffer => new(_buffer, 0, Length);

    /// <summary>
    /// Creates a new FusionWriter with a max size of <see cref="MaxSize"/>.
    /// </summary>
    /// <returns></returns>
    public static FusionWriter Create()
    {
        return new FusionWriter
        {
            _buffer = ArrayPool<byte>.Shared.Rent(MaxSize),
            Position = 0,
        };
    }

    public void Write<T>(T value) where T : IFusionSerializable
    {
        value.Serialize(this);
    }

    public void Write(GameObject gameObject)
    {
        if (gameObject != null)
            Write(gameObject.GetFullPath());
        else
            Write("null");
    }

    public void Write(Version version)
    {
        Write(version.Major);
        Write(version.Minor);
        Write(version.Build);
    }

    public void Write(Color color)
    {
        Write(color.r);
        Write(color.g);
        Write(color.b);
        Write(color.a);
    }

    public void Write(byte value)
    {
        _buffer[Position++] = value;
    }

    public void Write(byte? value)
    {
        Write(value.HasValue);

        if (value.HasValue)
            Write(value.Value);
    }

    public void Write(bool value)
    {
        Write((byte)(value ? 1u : 0u));
    }

    public void Write(double value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value);
        Position += 8;
    }

    public void Write(short value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value);
        Position += 2;
    }

    public void Write(int value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value);
        Position += 4;
    }

    public void Write(long value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value);
        Position += 8;
    }

    public void Write(sbyte value)
    {
        Write(value.ToByte());
    }

    public void Write(float value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value);
        Position += 4;
    }

    public void Write(Vector3 value)
    {
        Write(value.x);
        Write(value.y);
        Write(value.z);
    }

    public void Write(Vector2 value)
    {
        Write(value.x);
        Write(value.y);
    }

    public void Write(ushort value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value);
        Position += 2;
    }

    public void Write(ushort? value)
    {
        Write(value.HasValue);

        if (value.HasValue)
            Write(value.Value);
    }

    public void Write(uint value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value);
        Position += 4;
    }

    public void Write(ulong value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value);
        Position += 8;
    }

    public void Write(string value)
    {
        Write(value, Encoding.UTF8);
    }

    public void Write(string value, Encoding encoding)
    {
        int byteCount = encoding.GetByteCount(value);

        BigEndianHelper.WriteBytes(_buffer, Position, byteCount);
        encoding.GetBytes(value, 0, value.Length, _buffer, Position + 4);
        Position += 4 + byteCount;
    }

    public void Write(byte[] value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Length);
        System.Buffer.BlockCopy(value, 0, _buffer, Position + 4, value.Length);
        Position += 4 + value.Length;
    }

    public void Write(ArraySegment<byte> value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Count);
        System.Buffer.BlockCopy(value.Array, 0, _buffer, Position + 4, value.Count);
        Position += 4 + value.Count;
    }

    public void Write(char[] value)
    {
        Write(value, Encoding.UTF8);
    }

    public void Write(char[] value, Encoding encoding)
    {
        int byteCount = encoding.GetByteCount(value);

        BigEndianHelper.WriteBytes(_buffer, Position, byteCount);
        encoding.GetBytes(value, 0, value.Length, _buffer, Position + 4);
        Position += 4 + byteCount;
    }

    public void Write(ICollection<bool> value)
    {
        int num = (int)Math.Ceiling((double)value.Count / 8.0);

        BigEndianHelper.WriteBytes(_buffer, Position, value.Count);
        int num2 = 0;
        for (int i = 0; i < num; i++)
        {
            byte b = 0;
            int num3 = 7;
            while (num3 >= 0 && num2 < value.Count)
            {
                if (value.ElementAt(num2))
                {
                    b = (byte)(b | (byte)(1 << num3));
                }
                num2++;
                num3--;
            }
            _buffer[Position + 4 + i] = b;
        }
        Position += 4 + num;
    }

    public void Write(ICollection<double> value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Count);
        int num = 0;
        int num2 = Position + 4;
        while (num < value.Count)
        {
            byte[] bytes = BitConverter.GetBytes(value.ElementAt(num));
            if (BitConverter.IsLittleEndian)
            {
                System.Array.Reverse(bytes);
            }
            System.Buffer.BlockCopy(bytes, 0, _buffer, num2, 8);
            num++;
            num2 += 8;
        }
        Position += 4 + value.Count * 8;
    }

    public void Write(ICollection<short> value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Count);
        int num = 0;
        int num2 = Position + 4;
        while (num < value.Count)
        {
            BigEndianHelper.WriteBytes(_buffer, num2, value.ElementAt(num));
            num++;
            num2 += 2;
        }
        Position += 4 + value.Count * 2;
    }

    public void Write(ICollection<int> value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Count);
        int num = 0;
        int num2 = Position + 4;
        while (num < value.Count)
        {
            BigEndianHelper.WriteBytes(_buffer, num2, value.ElementAt(num));
            num++;
            num2 += 4;
        }
        Position += 4 + value.Count * 4;
    }

    public void Write(ICollection<long> value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Count);
        int num = 0;
        int num2 = Position + 4;
        while (num < value.Count)
        {
            BigEndianHelper.WriteBytes(_buffer, num2, value.ElementAt(num));
            num++;
            num2 += 8;
        }
        Position += 4 + value.Count * 8;
    }

    public void Write(sbyte[] value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Length);
        System.Buffer.BlockCopy(value, 0, _buffer, Position + 4, value.Length);
        Position += 4 + value.Length;
    }

    public void Write(ICollection<float> value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Count);
        int num = 0;
        int num2 = Position + 4;
        while (num < value.Count)
        {
            byte[] bytes = BitConverter.GetBytes(value.ElementAt(num));
            if (BitConverter.IsLittleEndian)
            {
                System.Array.Reverse(bytes);
            }
            System.Buffer.BlockCopy(bytes, 0, _buffer, num2, 4);
            num++;
            num2 += 4;
        }
        Position += 4 + value.Count * 4;
    }

    public void Write(Dictionary<string, string> value)
    {
        Write(value.Keys);
        Write(value.Values);
    }

    public void Write(ICollection<string> value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Count);
        Position += 4;

        for (var i = 0; i < value.Count; i++)
        {
            Write(value.ElementAt(i));
        }
    }

    public void Write(ICollection<ushort> value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Count);
        int num = 0;
        int num2 = Position + 4;
        while (num < value.Count)
        {
            BigEndianHelper.WriteBytes(_buffer, num2, value.ElementAt(num));
            num++;
            num2 += 2;
        }
        Position += 4 + value.Count * 2;
    }

    public void Write(ICollection<uint> value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Count);
        int num = 0;
        int num2 = Position + 4;
        while (num < value.Count)
        {
            BigEndianHelper.WriteBytes(_buffer, num2, value.ElementAt(num));
            num++;
            num2 += 4;
        }
        Position += 4 + value.Count * 4;
    }

    public void Write(ICollection<ulong> value)
    {
        BigEndianHelper.WriteBytes(_buffer, Position, value.Count);
        int num = 0;
        int num2 = Position + 4;
        while (num < value.Count)
        {
            BigEndianHelper.WriteBytes(_buffer, num2, value.ElementAt(num));
            num++;
            num2 += 8;
        }
        Position += 4 + value.Count * 8;
    }

    public void WriteRaw(byte[] bytes, int offset, int length)
    {
        System.Buffer.BlockCopy(bytes, offset, _buffer, Position, length);
        Position += length;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        ArrayPool<byte>.Shared.Return(_buffer);
    }
}