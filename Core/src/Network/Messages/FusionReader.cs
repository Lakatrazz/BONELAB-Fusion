using LabFusion.Data;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Network
{
    public class FusionReader : IDisposable {
        private byte[] buffer;

        private int Offset = 0;

        public int Length
        {
            get
            {
                return buffer.Length;
            }
        }

        public int Position { get; set; }

        public static FusionReader Create(byte[] buffer)
        {
            var reader = new FusionReader
            {
                buffer = buffer,
                Position = 0
            };
            return reader;
        }

        public T ReadFusionSerializable<T>() where T : IFusionSerializable, new() {
            T instance = new T();
            instance.Deserialize(this);
            return instance;
        }

        public void ReadFusionSerializable<T>(ref T value) where T : IFusionSerializable {
            value.Deserialize(this);
        }

        public IFusionSerializable ReadFusionSerializable(Type type)
        {
            var instance = Activator.CreateInstance(type) as IFusionSerializable;
            instance.Deserialize(this);
            return instance;
        }

        public void ReadFusionSerializable(ref IFusionSerializable value) {
            value.Deserialize(this);
        }

        /// <summary>
        /// Reads a gameObject from the reader. This is not always accurate.
        /// </summary>
        /// <returns></returns>
        public GameObject ReadGameObject() {
            string path = ReadString();
            if (path == "null") {
                return null;
            }
            else
                return GameObjectUtilities.GetGameObject(path);
        }

        /// <summary>
        /// Reads a version from the reader.
        /// </summary>
        /// <returns></returns>
        public Version ReadVersion() {
            return new Version(
                ReadInt32(),
                ReadInt32(),
                ReadInt32());
        }

        /// <summary>
        /// Reads a single color from the reader.
        /// </summary>
        /// <returns></returns>
        public Color ReadColor()
        {
            return new Color()
            {
                r = ReadSingle(),
                g = ReadSingle(),
                b = ReadSingle(),
                a = ReadSingle(),
            };
        }

        /// <summary>
        ///     Reads a single byte from the reader.
        /// </summary>
        /// <returns>The byte read.</returns>
        public byte ReadByte()
        {
            if (Position >= Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 1 byte but reader only has {0} bytes remaining.", Length - Position));
            }
            return buffer[Offset + Position++];
        }

        /// <summary>
        /// Reads a nullable byte from the reader.
        /// </summary>
        /// <returns></returns>
        public byte? ReadByteNullable() {
            bool hasValue = ReadBoolean();

            if (hasValue)
                return ReadByte();
            else
                return null;
        }

        /// <summary>
        /// Reads a nullable unsigned 16 bit integer from the reader.
        /// </summary>
        /// <returns></returns>
        public ushort? ReadUInt16Nullable() {
            bool hasValue = ReadBoolean();

            if (hasValue)
                return ReadUInt16();
            else
                return null;
        }

        /// <summary>
        ///     Reads a single boolean from the reader.
        /// </summary>
        /// <returns>The boolean read.</returns>
        public bool ReadBoolean()
        {
            return ReadByte() == 1;
        }

        /// <summary>
        ///     Reads a single double from the reader.
        /// </summary>
        /// <returns>The double read.</returns>
        public double ReadDouble()
        {
            if (Position + 8 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 8 bytes but reader only has {0} bytes remaining.", Length - Position));
            }
            double result = BigEndianHelper.ReadDouble(buffer, Offset + Position);
            Position += 8;
            return result;
        }

        /// <summary>
        ///     Reads a single 16bit integer from the reader.
        /// </summary>
        /// <returns>The 16bit integer read.</returns>
        public short ReadInt16()
        {
            if (Position + 2 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 2 bytes but reader only has {0} bytes remaining.", Length - Position));
            }
            short result = BigEndianHelper.ReadInt16(buffer, Offset + Position);
            Position += 2;
            return result;
        }

        /// <summary>
        ///     Reads a single 32bit integer from the reader.
        /// </summary>
        /// <returns>The 32bit integer read.</returns>
        public int ReadInt32()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 4 bytes but reader only has {0} bytes remaining.", Length - Position));
            }
            int result = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            Position += 4;
            return result;
        }

        /// <summary>
        ///     Reads a single 64bit integer from the reader.
        /// </summary>
        /// <returns>The 64bit integer read.</returns>
        public long ReadInt64()
        {
            if (Position + 8 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 8 bytes but reader only has {0} bytes remaining.", Length - Position));
            }
            long result = BigEndianHelper.ReadInt64(buffer, Offset + Position);
            Position += 8;
            return result;
        }

        /// <summary>
        ///     Reads a single signed byte from the reader.
        /// </summary>
        /// <returns>The signed byte read.</returns>
        public sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        /// <summary>
        ///     Reads a single single from the reader.
        /// </summary>
        /// <returns>The single read.</returns>
        public float ReadSingle()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 4 bytes but reader only has {0} bytes remaining.", Length - Position));
            }
            float result = BigEndianHelper.ReadSingle(buffer, Offset + Position);
            Position += 4;
            return result;
        }

        public Vector3 ReadVector3() {
            return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }

        public Vector2 ReadVector2() {
            return new Vector2(ReadSingle(), ReadSingle());
        }

        /// <summary>
        ///     Reads a single string from the reader using the reader's encoding.
        /// </summary>
        /// <returns>The string read.</returns>
        public string ReadString()
        {
            return ReadString(Encoding.UTF8);
        }

        /// <summary>
        ///     Reads a single string from the reader using the given encoding.
        /// </summary>
        /// <param name="encoding">The encoding to deserialize the string using.</param>
        /// <returns>The string read.</returns>
        public string ReadString(Encoding encoding)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num, Length - Position - 4));
            }
            string @string = encoding.GetString(buffer, Offset + Position + 4, num);
            Position += 4 + num;
            return @string;
        }

        /// <summary>
        ///     Reads a single unsigned 16bit integer from the reader.
        /// </summary>
        /// <returns>The unsigned 16bit integer read.</returns>
        public ushort ReadUInt16()
        {
            if (Position + 2 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 2 bytes but reader only has {0} bytes remaining.", Length - Position));
            }
            ushort result = BigEndianHelper.ReadUInt16(buffer, Offset + Position);
            Position += 2;
            return result;
        }

        /// <summary>
        ///     Reads a single unsigned 32bit integer from the reader.
        /// </summary>
        /// <returns>The unsigned 32bit integer read.</returns>
        public uint ReadUInt32()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 4 bytes but reader only has {0} bytes remaining.", Length - Position));
            }
            uint result = BigEndianHelper.ReadUInt32(buffer, Offset + Position);
            Position += 4;
            return result;
        }

        /// <summary>
        ///     Reads a single unsigned 64bit integer from the reader.
        /// </summary>
        /// <returns>The unsigned 64bit integer read.</returns>
        public ulong ReadUInt64()
        {
            if (Position + 8 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected 8 bytes but reader only has {0} bytes remaining.", Length - Position));
            }
            ulong result = BigEndianHelper.ReadUInt64(buffer, Offset + Position);
            Position += 8;
            return result;
        }

        /// <summary>
        ///     Reads an array of bytes from the reader.
        /// </summary>
        /// <returns>The array of bytes read.</returns>
        public byte[] ReadBytes()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num, Length - Position - 4));
            }
            byte[] array = ByteRetriever.Rent(num);
            Buffer.BlockCopy(buffer, Offset + Position + 4, array, 0, num);
            Position += 4 + num;
            return array;
        }

        /// <summary>
        ///     Reads an array of bytes from the reader.
        /// </summary>
        /// <param name="destination">The array to read bytes into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadBytesInto(byte[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num, Length - Position - 4));
            }
            Buffer.BlockCopy(buffer, Offset + Position + 4, destination, 0, num);
            Position += 4 + num;
        }

        /// <summary>
        ///     Reads a array of characters from the reader.
        /// </summary>
        /// <returns>The array of characters read.</returns>
        public char[] ReadChars()
        {
            return ReadChars(Encoding.UTF8);
        }

        /// <summary>
        ///     Reads a array of characters from the reader.
        /// </summary>
        /// <param name="destination">The array to read characters into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadCharsInto(char[] destination, int offset)
        {
            ReadCharsInto(destination, offset, Encoding.UTF8);
        }

        /// <summary>
        ///     Reads an array of characters from the reader using the given encoding.
        /// </summary>
        /// <param name="encoding">The encoding to use during the deserialization.</param>
        /// <returns>The array of characters read.</returns>
        public char[] ReadChars(Encoding encoding)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num, Length - Position - 4));
            }
            char[] chars = encoding.GetChars(buffer, Offset + Position + 4, num);
            Position += 4 + num;
            return chars;
        }

        /// <summary>
        ///     Reads an array of characters from the reader using the given encoding.
        /// </summary>
        /// <param name="destination">The array to read characters into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        /// <param name="encoding">The encoding to use during the deserialization.</param>
        public void ReadCharsInto(char[] destination, int offset, Encoding encoding)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num, Length - Position - 4));
            }
            encoding.GetChars(buffer, Offset + Position + 4, num, destination, offset);
            Position += 4 + num;
        }

        /// <summary>
        ///     Reads an array of booleans from the reader.
        /// </summary>
        /// <returns>The array of booleans read.</returns>
        public bool[] ReadBooleans()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            int num2 = (int)Math.Ceiling((double)num / 8.0);
            if (Position + 4 + num2 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num2, Length - Position - 4));
            }
            bool[] array = new bool[num];
            int num3 = 0;
            for (int i = 0; i < num2; i++)
            {
                byte b = buffer[Offset + Position + 4 + i];
                int num4 = 7;
                while (num4 >= 0 && num3 < num)
                {
                    array[num3++] = (b & (1 << num4)) != 0;
                    num4--;
                }
            }
            Position += 4 + num2;
            return array;
        }

        /// <summary>
        ///     Reads an array of booleans from the reader.
        /// </summary>
        /// <param name="destination">The array to read booleans into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadBooleansInto(bool[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            int num2 = (int)Math.Ceiling((double)num / 8.0);
            if (Position + 4 + num2 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num2, Length - Position - 4));
            }
            int num3 = offset;
            for (int i = 0; i < num2; i++)
            {
                byte b = buffer[Offset + Position + 4 + i];
                int num4 = 7;
                while (num4 >= 0 && num3 < num)
                {
                    destination[num3++] = (b & (1 << num4)) != 0;
                    num4--;
                }
            }
            Position += 4 + num2;
        }

        /// <summary>
        ///     Reads an array of doubles from the reader.
        /// </summary>
        /// <returns>The array of doubles read.</returns>
        public double[] ReadDoubles()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 8 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 8, Length - Position - 4));
            }
            double[] array = new double[num];
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                array[num2] = BigEndianHelper.ReadDouble(buffer, num3);
                num2++;
                num3 += 8;
            }
            Position += 4 + num * 8;
            return array;
        }

        /// <summary>
        ///     Reads an array of doubles from the reader.
        /// </summary>
        /// <returns>The array of doubles read.</returns>
        /// <param name="destination">The array to read doubles into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadDoublesInto(double[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 8 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 8, Length - Position - 4));
            }
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                destination[num2 + offset] = BigEndianHelper.ReadDouble(buffer, num3);
                num2++;
                num3 += 8;
            }
            Position += 4 + num * 8;
        }

        /// <summary>
        ///     Reads an array of 16bit integers from the reader.
        /// </summary>
        /// <returns>The array of 16bit integers read.</returns>
        public short[] ReadInt16s()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 2 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 2, Length - Position - 4));
            }
            short[] array = new short[num];
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                array[num2] = BigEndianHelper.ReadInt16(buffer, num3);
                num2++;
                num3 += 2;
            }
            Position += 4 + num * 2;
            return array;
        }

        /// <summary>
        ///     Reads an array of 16bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read int16s into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadInt16sInto(short[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 2 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 2, Length - Position - 4));
            }
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                destination[num2 + offset] = BigEndianHelper.ReadInt16(buffer, num3);
                num2++;
                num3 += 2;
            }
            Position += 4 + num * 2;
        }

        /// <summary>
        ///     Reads an array of 32bit integers from the reader.
        /// </summary>
        /// <returns>The array of 32bit integers read.</returns>
        public int[] ReadInt32s()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 4, Length - Position - 4));
            }
            int[] array = new int[num];
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                array[num2] = BigEndianHelper.ReadInt32(buffer, num3);
                num2++;
                num3 += 4;
            }
            Position += 4 + num * 4;
            return array;
        }

        /// <summary>
        ///     Reads an array of 32bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read int32s into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadInt32sInto(int[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 4, Length - Position - 4));
            }
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                destination[num2 + offset] = BigEndianHelper.ReadInt32(buffer, num3);
                num2++;
                num3 += 4;
            }
            Position += 4 + num * 4;
        }

        /// <summary>
        ///     Reads an array of 64bit integers from the reader.
        /// </summary>
        /// <returns>The array of 64bit integers read.</returns>
        public long[] ReadInt64s()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 8 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 8, Length - Position - 4));
            }
            long[] array = new long[num];
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                array[num2] = BigEndianHelper.ReadInt64(buffer, num3);
                num2++;
                num3 += 8;
            }
            Position += 4 + num * 8;
            return array;
        }

        /// <summary>
        ///     Reads an array of 64bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read int64s into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadInt64sInto(long[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 8 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 8, Length - Position - 4));
            }
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                destination[num2 + offset] = BigEndianHelper.ReadInt64(buffer, num3);
                num2++;
                num3 += 8;
            }
            Position += 4 + num * 8;
        }

        /// <summary>
        ///     Reads an array of signed bytes from the reader.
        /// </summary>
        /// <returns>The array of signed bytes read.</returns>
        public sbyte[] ReadSBytes()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num, Length - Position - 4));
            }
            sbyte[] array = new sbyte[num];
            Buffer.BlockCopy(buffer, Offset + Position + 4, array, 0, num);
            Position += 4 + num;
            return array;
        }

        /// <summary>
        ///     Reads an array of signed bytes from the reader.
        /// </summary>
        /// <param name="destination">The array to read sbytes into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadSBytesInto(sbyte[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num, Length - Position - 4));
            }
            Buffer.BlockCopy(buffer, Offset + Position + 4, destination, offset, num);
            Position += 4 + num;
        }

        /// <summary>
        ///     Reads an array of singles from the reader.
        /// </summary>
        /// <returns>The array of singles read.</returns>
        public float[] ReadSingles()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 4, Length - Position - 4));
            }
            float[] array = new float[num];
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                array[num2] = BigEndianHelper.ReadSingle(buffer, num3);
                num2++;
                num3 += 4;
            }
            Position += 4 + num * 4;
            return array;
        }

        /// <summary>
        ///     Reads an array of singles from the reader.
        /// </summary>
        /// <param name="destination">The array to read singles into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadSinglesInto(float[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 4, Length - Position - 4));
            }
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                destination[num2 + offset] = BigEndianHelper.ReadSingle(buffer, num3);
                num2++;
                num3 += 4;
            }
            Position += 4 + num * 4;
        }

        /// <summary>
        ///     Reads an array of strings from the reader using the reader's encoding.
        /// </summary>
        /// <returns>The array of strings read.</returns>
        public string[] ReadStrings()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            Position += 4;
            string[] array = new string[num];
            for (int i = 0; i < num; i++)
            {
                array[i] = ReadString();
            }
            return array;
        }

        /// <summary>
        ///     Reads an array of strings from the reader using the reader's encoding.
        /// </summary>
        /// <param name="destination">The array to read strings into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadStringsInto(string[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            Position += 4;
            for (int i = 0; i < num; i++)
            {
                destination[i + offset] = ReadString();
            }
        }

        /// <summary>
        /// Reads a dictionary of strings from the reader.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> ReadStringDictionary()
        {
            var dictionary = new Dictionary<string, string>();
            var keys = ReadStrings();
            var values = ReadStrings();

            for (var i = 0; i < keys.Length; i++) {
                dictionary.Add(keys[i], values[i]);
            }

            return dictionary;
        }

        /// <summary>
        ///     Reads an array unsigned 16bit integers from the reader.
        /// </summary>
        /// <returns>The array of unsigned 16bit integers read.</returns>
        public ushort[] ReadUInt16s()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 2 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 2, Length - Position - 4));
            }
            ushort[] array = new ushort[num];
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                array[num2] = BigEndianHelper.ReadUInt16(buffer, num3);
                num2++;
                num3 += 2;
            }
            Position += 4 + num * 2;
            return array;
        }

        /// <summary>
        ///     Reads an array unsigned 16bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read strings into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadUInt16sInto(ushort[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 2 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 2, Length - Position - 4));
            }
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                destination[num2 + offset] = BigEndianHelper.ReadUInt16(buffer, num3);
                num2++;
                num3 += 2;
            }
            Position += 4 + num * 2;
        }

        /// <summary>
        ///     Reads an array unsigned 32bit integers from the reader.
        /// </summary>
        /// <returns>The array of unsigned 32bit integers read.</returns>
        public uint[] ReadUInt32s()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 4, Length - Position - 4));
            }
            uint[] array = new uint[num];
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                array[num2] = BigEndianHelper.ReadUInt32(buffer, num3);
                num2++;
                num3 += 4;
            }
            Position += 4 + num * 4;
            return array;
        }

        /// <summary>
        ///     Reads an array unsigned 32bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read strings into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadUInt32sInto(uint[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 4, Length - Position - 4));
            }
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                destination[num2 + offset] = BigEndianHelper.ReadUInt32(buffer, num3);
                num2++;
                num3 += 4;
            }
            Position += 4 + num * 4;
        }

        /// <summary>
        ///     Reads an array unsigned 64bit integers from the reader.
        /// </summary>
        /// <returns>The array of unsigned 64bit integers read.</returns>
        public ulong[] ReadUInt64s()
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 8 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 8, Length - Position - 4));
            }
            ulong[] array = new ulong[num];
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                array[num2] = BigEndianHelper.ReadUInt64(buffer, num3);
                num2++;
                num3 += 8;
            }
            Position += 4 + num * 8;
            return array;
        }

        /// <summary>
        ///     Reads an array unsigned 64bit integers from the reader.
        /// </summary>
        /// <param name="destination">The array to read strings into.</param>
        /// <param name="offset">The offset at which to write bytes into the array.</param>
        public void ReadUInt64sInto(ulong[] destination, int offset)
        {
            if (Position + 4 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read as the reader does not have enough data remaining. Expected 4 byte array length header but reader only has {0} bytes remaining.", Length - Position));
            }
            int num = BigEndianHelper.ReadInt32(buffer, Offset + Position);
            if (Position + 4 + num * 8 > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", num * 8, Length - Position - 4));
            }
            int num2 = 0;
            int num3 = Offset + Position + 4;
            while (num2 < num)
            {
                destination[num2 + offset] = BigEndianHelper.ReadUInt64(buffer, num3);
                num2++;
                num3 += 8;
            }
            Position += 4 + num * 8;
        }

        /// <summary>
        ///     Reads an array of raw bytes from the reader.
        /// </summary>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The array of bytes read.</returns>
        public byte[] ReadRaw(int length)
        {
            if (Position + length > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", length, Length - Position));
            }
            byte[] result = ByteRetriever.Rent(length);
            ReadRawInto(result, 0, length);
            return result;
        }

        /// <summary>
        ///     Reads an array of raw bytes from the reader into the given array.
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="offset">The offset to start writing into the buffer at.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>The array of bytes read.</returns>
        public void ReadRawInto(byte[] buffer, int offset, int length)
        {
            if (Position + length > Length)
            {
                throw new IndexOutOfRangeException(string.Format("Failed to read data from reader as the reader does not have enough data remaining. Expected {0} bytes but reader only has {1} bytes remaining.", length, Length - Position));
            }
            Buffer.BlockCopy(this.buffer, this.Offset + Position, buffer, offset, length);
            Position += length;
        }


        public void Dispose() {
            GC.SuppressFinalize(this);
        }
    }
}
