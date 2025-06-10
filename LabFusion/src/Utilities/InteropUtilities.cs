namespace LabFusion.Utilities;

/// <summary>
/// Methods for calling into Il2Cpp functions faster.
/// </summary>
public static class InteropUtilities
{
    /// <summary>
    /// Writes to an Il2Cpp float struct array with cached pointers and no safety checks.
    /// </summary>
    /// <param name="pointer">The pointer of the array.</param>
    /// <param name="size"><see cref="IntPtr.Size"/>.</param>
    /// <param name="index">The index of the array.</param>
    /// <param name="value">The value being written.</param>
    public static unsafe void FloatArrayFastWrite(IntPtr pointer, int size, int index, float value)
    {
        *(float*)((byte*)IntPtr.Add(pointer, 4 * size).ToPointer() + (nint)index * (nint)sizeof(float)) = value;
    }

    /// <summary>
    /// Reads from an Il2Cpp float struct array with cached pointers and no safety checks.
    /// </summary>
    /// <param name="pointer">The pointer of the array.</param>
    /// <param name="size"><see cref="IntPtr.Size"/>.</param>
    /// <param name="index">The index of the array.</param>
    /// <returns>The float at the index.</returns>
    public static unsafe float FloatArrayFastRead(IntPtr pointer, int size, int index)
    {
        return *(float*)((byte*)IntPtr.Add(pointer, 4 * size).ToPointer() + (nint)index * (nint)sizeof(float));
    }

    /// <summary>
    /// Copies all floats from an Il2Cpp float struct array into a managed float array.
    /// </summary>
    /// <param name="pointer">The pointer of the array.</param>
    /// <param name="size"><see cref="IntPtr.Size"/>.</param>
    /// <param name="length">The length of the array.</param>
    /// <param name="array">The managed array to copy into.</param>
    public static void Copy(IntPtr pointer, int size, int length, float[] array)
    {
        for (var i = 0; i < length; i++)
        {
            array[i] = FloatArrayFastRead(pointer, size, i);
        }
    }
}
