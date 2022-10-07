using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Extensions;
using LabFusion.Utilities;
using LabFusion.Data;

namespace LabFusion.Network
{
    public class FusionWriter : IDisposable
    {
        public int Position { get; set; }

        public int Length {
            get {
                return buffer.Length;
            }
        }


        private byte[] buffer;

        public int Capacity
        {
            get
            {
                return buffer.Length;
            }
        }

        public byte[] Buffer {
            get {
                return buffer;
            }
        }

        public static FusionWriter Create()
        {
            return Create(16);
        }

        public static FusionWriter Create(int initialCapacity)
        {
            return new FusionWriter
            {
                buffer = new byte[initialCapacity],
                Position = 0
            };
        }

        public T Write<T>() where T : IFusionSerializable, new() {
            T instance = new T();
            instance.Serialize(this);
            return instance;
        }

        public void Write(byte value)
        {
            buffer.EnsureLength(Position + 1);
            buffer[Position++] = value;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(bool value)
        {
            buffer.EnsureLength(Position + 1);
            Write((byte)(value ? 1u : 0u));
        }

        public void Write(double value)
        {
            buffer.EnsureLength(Position + 8);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 8;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(short value)
        {
            buffer.EnsureLength(Position + 2);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 2;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(int value)
        {
            buffer.EnsureLength(Position + 4);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 4;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(long value)
        {
            buffer.EnsureLength(Position + 8);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 8;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(sbyte value)
        {
            Write((byte)value);
        }

        public void Write(float value)
        {
            buffer.EnsureLength(Position + 4);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 4;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(ushort value)
        {
            buffer.EnsureLength(Position + 2);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 2;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(uint value)
        {
            buffer.EnsureLength(Position + 4);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 4;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(ulong value)
        {
            buffer.EnsureLength(Position + 8);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 8;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(string value)
        {
            Write(value, Encoding.UTF8);
        }

        public void Write(string value, Encoding encoding)
        {
            int byteCount = encoding.GetByteCount(value);
            buffer.EnsureLength(Position + 4 + byteCount);
            BigEndianHelper.WriteBytes(buffer, Position, byteCount);
            encoding.GetBytes(value, 0, value.Length, buffer, Position + 4);
            Position += 4 + byteCount;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(byte[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            System.Buffer.BlockCopy(value, 0, buffer, Position + 4, value.Length);
            Position += 4 + value.Length;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(char[] value)
        {
            Write(value, Encoding.UTF8);
        }

        public void Write(char[] value, Encoding encoding)
        {
            int byteCount = encoding.GetByteCount(value);
            buffer.EnsureLength(Position + 4 + byteCount);
            BigEndianHelper.WriteBytes(buffer, Position, byteCount);
            encoding.GetBytes(value, 0, value.Length, buffer, Position + 4);
            Position += 4 + byteCount;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(bool[] value)
        {
            int num = (int)Math.Ceiling((double)value.Length / 8.0);
            buffer.EnsureLength(Position + 4 + num);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            int num2 = 0;
            for (int i = 0; i < num; i++)
            {
                byte b = 0;
                int num3 = 7;
                while (num3 >= 0 && num2 < value.Length)
                {
                    if (value[num2])
                    {
                        b = (byte)(b | (byte)(1 << num3));
                    }
                    num2++;
                    num3--;
                }
                buffer[Position + 4 + i] = b;
            }
            Position += 4 + num;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(double[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 8);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Length)
            {
                byte[] bytes = BitConverter.GetBytes(value[num]);
                if (BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(bytes);
                }
                System.Buffer.BlockCopy(bytes, 0, buffer, num2, 8);
                num++;
                num2 += 8;
            }
            Position += 4 + value.Length * 8;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(short[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 2);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Length)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value[num]);
                num++;
                num2 += 2;
            }
            Position += 4 + value.Length * 2;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(int[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 4);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Length)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value[num]);
                num++;
                num2 += 4;
            }
            Position += 4 + value.Length * 4;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(long[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 8);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Length)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value[num]);
                num++;
                num2 += 8;
            }
            Position += 4 + value.Length * 8;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(sbyte[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            System.Buffer.BlockCopy(value, 0, buffer, Position + 4, value.Length);
            Position += 4 + value.Length;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(float[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 4);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Length)
            {
                byte[] bytes = BitConverter.GetBytes(value[num]);
                if (BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(bytes);
                }
                System.Buffer.BlockCopy(bytes, 0, buffer, num2, 4);
                num++;
                num2 += 4;
            }
            Position += 4 + value.Length * 4;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(string[] value)
        {
            buffer.EnsureLength(Position + 4);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            Position += 4;
            buffer.EnsureLength(Math.Max(Length, Position));
            foreach (string value2 in value)
            {
                Write(value2);
            }
        }

        public void Write(ushort[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 2);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Length)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value[num]);
                num++;
                num2 += 2;
            }
            Position += 4 + value.Length * 2;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(uint[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 4);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Length)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value[num]);
                num++;
                num2 += 4;
            }
            Position += 4 + value.Length * 4;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Write(ulong[] value)
        {
            buffer.EnsureLength(Position + 4 + value.Length * 8);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Length)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value[num]);
                num++;
                num2 += 8;
            }
            Position += 4 + value.Length * 8;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void WriteRaw(byte[] bytes, int offset, int length)
        {
            buffer.EnsureLength(Position + length);
            System.Buffer.BlockCopy(bytes, offset, buffer, Position, length);
            Position += length;
            buffer.EnsureLength(Math.Max(Length, Position));
        }

        public void Dispose() { }
    }
}
