using System.Runtime.InteropServices;

namespace Il2CppInterop.Runtime.Attributes
{
    public sealed class HideFromIl2CppAttribute : Attribute { }
}

namespace MelonLoader
{
    public sealed class RegisterTypeInIl2CppAttribute : Attribute { }
}

namespace UnityEngine
{
    public class MonoBehaviour { public MonoBehaviour(IntPtr pointer) { } }
}

namespace Il2CppInterop.Runtime.InteropTypes.Arrays
{
    public sealed class Il2CppStructArray<T> : IDisposable where T : struct
    {
        private GCHandle _handle;
        public T[] Values { get; }
        public int Length => Values.Length;
        public IntPtr Pointer => _handle.AddrOfPinnedObject();
        public Il2CppStructArray(T[] values)
        {
            Values = values;
            _handle = GCHandle.Alloc(values, GCHandleType.Pinned);
        }
        public void Dispose() => _handle.Free();
    }
}

namespace LabFusion.Utilities
{
    public static class InteropUtilities
    {
        public static float FloatArrayFastRead(IntPtr pointer, int pointerSize, int index) => BitConverter.Int32BitsToSingle(Marshal.ReadInt32(pointer, index * sizeof(float)));
        public static void FloatArrayFastWrite(IntPtr pointer, int pointerSize, int index, float value) => Marshal.WriteInt32(pointer, index * sizeof(float), BitConverter.SingleToInt32Bits(value));
    }
}

namespace LabFusion.Math
{
    public static class ManagedMathf { public static float Lerp(float a, float b, float t) => a + (b - a) * t; }
    public static class Smoothing { public static float CalculateDecay(float speed, float delta) => 1f - MathF.Exp(-speed * delta); }
}

namespace LabFusion.Audio
{
    public static class AudioInfo
    {
        public const int OutputSampleRate = 48000;
        public const float ToneNormalizer = 1f;
    }
}
