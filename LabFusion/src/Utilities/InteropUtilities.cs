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
}
