using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities
{
    /// <summary>
    ///     Helper class for writing primitives to arrays in big endian format.
    /// </summary>
    internal static class BigEndianHelper
    {
        /// <summary>
        ///     Writes the bytes from the short to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        internal static void WriteBytes(byte[] destination, int offset, short value)
        {
            destination[offset] = (byte)(value >> 8);
            destination[offset + 1] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the unsigned short to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        internal static void WriteBytes(byte[] destination, int offset, ushort value)
        {
            destination[offset] = (byte)(value >> 8);
            destination[offset + 1] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the int to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        internal static void WriteBytes(byte[] destination, int offset, int value)
        {
            destination[offset] = (byte)(value >> 24);
            destination[offset + 1] = (byte)(value >> 16);
            destination[offset + 2] = (byte)(value >> 8);
            destination[offset + 3] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the unsigned int to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        internal static void WriteBytes(byte[] destination, int offset, uint value)
        {
            destination[offset] = (byte)(value >> 24);
            destination[offset + 1] = (byte)(value >> 16);
            destination[offset + 2] = (byte)(value >> 8);
            destination[offset + 3] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the long to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        internal static void WriteBytes(byte[] destination, int offset, long value)
        {
            destination[offset] = (byte)(value >> 56);
            destination[offset + 1] = (byte)(value >> 48);
            destination[offset + 2] = (byte)(value >> 40);
            destination[offset + 3] = (byte)(value >> 32);
            destination[offset + 4] = (byte)(value >> 24);
            destination[offset + 5] = (byte)(value >> 16);
            destination[offset + 6] = (byte)(value >> 8);
            destination[offset + 7] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the unsigned long to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        internal static void WriteBytes(byte[] destination, int offset, ulong value)
        {
            destination[offset] = (byte)(value >> 56);
            destination[offset + 1] = (byte)(value >> 48);
            destination[offset + 2] = (byte)(value >> 40);
            destination[offset + 3] = (byte)(value >> 32);
            destination[offset + 4] = (byte)(value >> 24);
            destination[offset + 5] = (byte)(value >> 16);
            destination[offset + 6] = (byte)(value >> 8);
            destination[offset + 7] = (byte)value;
        }

        /// <summary>
        ///     Writes the bytes from the float to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        internal unsafe static void WriteBytes(byte[] destination, int offset, float value)
        {
            float* ptr = &value;
            uint value2 = *(uint*)ptr;
            WriteBytes(destination, offset, value2);
        }

        /// <summary>
        ///     Writes the bytes from the double to the destination array at offset.
        /// </summary>
        /// <param name="destination">The array to write to.</param>
        /// <param name="offset">The position of the array to begin writing.</param>
        /// <param name="value">The value to write.</param>
        internal unsafe static void WriteBytes(byte[] destination, int offset, double value)
        {
            double* ptr = &value;
            ulong value2 = *(ulong*)ptr;
            WriteBytes(destination, offset, value2);
        }

        /// <summary>
        ///     Reads an short from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The short read.</returns>
        internal static short ReadInt16(byte[] source, int offset)
        {
            return (short)((source[offset] << 8) | source[offset + 1]);
        }

        /// <summary>
        ///     Reads an unsigned short from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The unsigned short read.</returns>
        internal static ushort ReadUInt16(byte[] source, int offset)
        {
            return (ushort)((source[offset] << 8) | source[offset + 1]);
        }

        /// <summary>
        ///     Reads an integer from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The integer read.</returns>
        internal static int ReadInt32(byte[] source, int offset)
        {
            return (source[offset] << 24) | (source[offset + 1] << 16) | (source[offset + 2] << 8) | source[offset + 3];
        }

        /// <summary>
        ///     Reads an unsigned integer from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The unsigned integer read.</returns>
        internal static uint ReadUInt32(byte[] source, int offset)
        {
            return (uint)((source[offset] << 24) | (source[offset + 1] << 16) | (source[offset + 2] << 8) | source[offset + 3]);
        }

        /// <summary>
        ///     Reads a long from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The long read.</returns>
        internal static long ReadInt64(byte[] source, int offset)
        {
            return (long)(((ulong)source[offset] << 56) | ((ulong)source[offset + 1] << 48) | ((ulong)source[offset + 2] << 40) | ((ulong)source[offset + 3] << 32) | ((ulong)source[offset + 4] << 24) | ((ulong)source[offset + 5] << 16) | ((ulong)source[offset + 6] << 8) | source[offset + 7]);
        }

        /// <summary>
        ///     Reads an unsigned long from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The unsigned long read.</returns>
        internal static ulong ReadUInt64(byte[] source, int offset)
        {
            return ((ulong)source[offset] << 56) | ((ulong)source[offset + 1] << 48) | ((ulong)source[offset + 2] << 40) | ((ulong)source[offset + 3] << 32) | ((ulong)source[offset + 4] << 24) | ((ulong)source[offset + 5] << 16) | ((ulong)source[offset + 6] << 8) | source[offset + 7];
        }

        /// <summary>
        ///     Reads a single from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The single read.</returns>
        internal unsafe static float ReadSingle(byte[] source, int offset)
        {
            uint num = ReadUInt32(source, offset);
            uint* ptr = &num;
            return *(float*)ptr;
        }

        /// <summary>
        ///     Reads a double from the array at offset.
        /// </summary>
        /// <param name="source">The array to read from.</param>
        /// <param name="offset">The position to begin reading from.</param>
        /// <returns>The double read.</returns>
        internal unsafe static double ReadDouble(byte[] source, int offset)
        {
            ulong num = ReadUInt64(source, offset);
            ulong* ptr = &num;
            return *(double*)ptr;
        }

        /// <summary>
        ///     Swaps the byte order of a ushort.
        /// </summary>
        /// <param name="value">The bytes to swap.</param>
        /// <returns>The reversed bytes.</returns>
        internal static ushort SwapBytes(ushort value)
        {
            return (ushort)(((value & 0xFF) << 8) | ((value & 0xFF00) >> 8));
        }

        /// <summary>
        ///     Swaps the byte order of a uint.
        /// </summary>
        /// <param name="value">The bytes to swap.</param>
        /// <returns>The reversed bytes.</returns>
        internal static uint SwapBytes(uint value)
        {
            return ((value & 0xFF) << 24) | ((value & 0xFF00) << 8) | ((value & 0xFF0000) >> 8) | ((value & 0xFF000000u) >> 24);
        }

        /// <summary>
        ///     Swaps the byte order of a ulong.
        /// </summary>
        /// <param name="value">The bytes to swap.</param>
        /// <returns>The reversed bytes.</returns>
        internal static ulong SwapBytes(ulong value)
        {
            return ((value & 0xFF) << 56) | ((value & 0xFF00) << 40) | ((value & 0xFF0000) << 24) | ((value & 0xFF000000u) << 8) | ((value & 0xFF00000000L) >> 8) | ((value & 0xFF0000000000L) >> 24) | ((value & 0xFF000000000000L) >> 40) | ((value & 0xFF00000000000000uL) >> 56);
        }
    }

}
