using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Extensions;
using LabFusion.Utilities;
using LabFusion.Data;

using UnityEngine;

#if DEBUG
using LabFusion.Debugging;
#endif

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

        public byte[] Buffer {
            get {
                return buffer;
            }
        }

        /// <summary>
        /// This is not recommended. Use Create(int initialCapacity) as resizing arrays costs performance.
        /// </summary>
        /// <returns></returns>
        public static FusionWriter Create()
        {
            return Create(ByteRetriever.DefaultSize);
        }

        /// <summary>
        /// This is the recommended version. If you know a minimum size of your message, it can save a lot of performance.
        /// </summary>
        /// <param name="initialCapacity"></param>
        /// <returns></returns>
        public static FusionWriter Create(int initialCapacity)
        {
            return new FusionWriter
            {
                buffer = ByteRetriever.Rent(initialCapacity),
                Position = 0
            };
        }

        public void Write<T>(T value) where T : IFusionSerializable {
            value.Serialize(this);
        }

        public void Write(GameObject gameObject)
        {
            if (gameObject != null)
                Write(gameObject.GetFullPath());
            else
                Write("null");
        }

        public void Write(Version version) {
            Write(version.Major);
            Write(version.Minor);
            Write(version.Build);
        }

        public void Write(Color color) {
            Write(color.r);
            Write(color.g);
            Write(color.b);
            Write(color.a);
        }

        public void Write(byte value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 1);
            buffer[Position++] = value;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(byte? value) {
            Write(value.HasValue);

            if (value.HasValue)
                Write(value.Value);
        }

        public void Write(bool value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 1);
            Write((byte)(value ? 1u : 0u));
        }

        public void Write(double value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 8);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 8;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(short value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 2);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 2;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(int value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 4;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(long value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 8);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 8;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(sbyte value)
        {
            Write((byte)value);
        }

        public void Write(float value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 4;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(Vector3 value) {
            Write(value.x);
            Write(value.y);
            Write(value.z);
        }

        public void Write(Vector2 value) {
            Write(value.x);
            Write(value.y);
        }

        public void Write(ushort value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 2);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 2;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(ushort? value)
        {
            Write(value.HasValue);

            if (value.HasValue)
                Write(value.Value);
        }

        public void Write(uint value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 4;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(ulong value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 8);
            BigEndianHelper.WriteBytes(buffer, Position, value);
            Position += 8;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(string value)
        {
            Write(value, Encoding.UTF8);
        }

        public void Write(string value, Encoding encoding)
        {
            int byteCount = encoding.GetByteCount(value);
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + byteCount);
            BigEndianHelper.WriteBytes(buffer, Position, byteCount);
            encoding.GetBytes(value, 0, value.Length, buffer, Position + 4);
            Position += 4 + byteCount;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(byte[] value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + value.Length);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            System.Buffer.BlockCopy(value, 0, buffer, Position + 4, value.Length);
            Position += 4 + value.Length;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(char[] value)
        {
            Write(value, Encoding.UTF8);
        }

        public void Write(char[] value, Encoding encoding)
        {
            int byteCount = encoding.GetByteCount(value);
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + byteCount);
            BigEndianHelper.WriteBytes(buffer, Position, byteCount);
            encoding.GetBytes(value, 0, value.Length, buffer, Position + 4);
            Position += 4 + byteCount;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(ICollection<bool> value)
        {
            int num = (int)Math.Ceiling((double)value.Count / 8.0);
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + num);
            BigEndianHelper.WriteBytes(buffer, Position, value.Count);
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
                buffer[Position + 4 + i] = b;
            }
            Position += 4 + num;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(ICollection<double> value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + value.Count * 8);
            BigEndianHelper.WriteBytes(buffer, Position, value.Count);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Count)
            {
                byte[] bytes = BitConverter.GetBytes(value.ElementAt(num));
                if (BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(bytes);
                }
                System.Buffer.BlockCopy(bytes, 0, buffer, num2, 8);
                num++;
                num2 += 8;
            }
            Position += 4 + value.Count * 8;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(ICollection<short> value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + value.Count * 2);
            BigEndianHelper.WriteBytes(buffer, Position, value.Count);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Count)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value.ElementAt(num));
                num++;
                num2 += 2;
            }
            Position += 4 + value.Count * 2;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(ICollection<int> value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + value.Count * 4);
            BigEndianHelper.WriteBytes(buffer, Position, value.Count);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Count)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value.ElementAt(num));
                num++;
                num2 += 4;
            }
            Position += 4 + value.Count * 4;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(ICollection<long> value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + value.Count * 8);
            BigEndianHelper.WriteBytes(buffer, Position, value.Count);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Count)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value.ElementAt(num));
                num++;
                num2 += 8;
            }
            Position += 4 + value.Count * 8;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(sbyte[] value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + value.Length);
            BigEndianHelper.WriteBytes(buffer, Position, value.Length);
            System.Buffer.BlockCopy(value, 0, buffer, Position + 4, value.Length);
            Position += 4 + value.Length;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(ICollection<float> value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + value.Count * 4);
            BigEndianHelper.WriteBytes(buffer, Position, value.Count);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Count)
            {
                byte[] bytes = BitConverter.GetBytes(value.ElementAt(num));
                if (BitConverter.IsLittleEndian)
                {
                    System.Array.Reverse(bytes);
                }
                System.Buffer.BlockCopy(bytes, 0, buffer, num2, 4);
                num++;
                num2 += 4;
            }
            Position += 4 + value.Count * 4;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(Dictionary<string, string> value) {
            Write(value.Keys);
            Write(value.Values);
        }

        public void Write(ICollection<string> value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4);
            BigEndianHelper.WriteBytes(buffer, Position, value.Count);
            Position += 4;
            ArrayExtensions.EnsureLength(ref buffer, Position);
            for (var i = 0; i < value.Count; i++) {
                Write(value.ElementAt(i));
            }
        }

        public void Write(ICollection<ushort> value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + value.Count * 2);
            BigEndianHelper.WriteBytes(buffer, Position, value.Count);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Count)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value.ElementAt(num));
                num++;
                num2 += 2;
            }
            Position += 4 + value.Count * 2;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(ICollection<uint> value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + value.Count * 4);
            BigEndianHelper.WriteBytes(buffer, Position, value.Count);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Count)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value.ElementAt(num));
                num++;
                num2 += 4;
            }
            Position += 4 + value.Count * 4;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void Write(ICollection<ulong> value)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + 4 + value.Count * 8);
            BigEndianHelper.WriteBytes(buffer, Position, value.Count);
            int num = 0;
            int num2 = Position + 4;
            while (num < value.Count)
            {
                BigEndianHelper.WriteBytes(buffer, num2, value.ElementAt(num));
                num++;
                num2 += 8;
            }
            Position += 4 + value.Count * 8;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        public void WriteRaw(byte[] bytes, int offset, int length)
        {
            ArrayExtensions.EnsureLength(ref buffer, Position + length);
            System.Buffer.BlockCopy(bytes, offset, buffer, Position, length);
            Position += length;
            ArrayExtensions.EnsureLength(ref buffer, Position);
        }

        internal void EnsureLength() {
            if (buffer.Length != Position) {
                Array.Resize(ref buffer, Position);

#if DEBUG
                if (FusionUnityLogger.EnableArrayResizeLogs)
#pragma warning disable CS0162 // Unreachable code detected
                    FusionLogger.Warn("A message's buffer length was not its position, causing a resize!");
#pragma warning restore CS0162 // Unreachable code detected
#endif
            }
        }

        public void Dispose() {
            GC.SuppressFinalize(this);

            ByteRetriever.Return(buffer);
        }
    }
}
