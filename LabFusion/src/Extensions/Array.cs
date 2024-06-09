#if DEBUG
using LabFusion.Debugging;
#endif

using LabFusion.Utilities;

using System.Runtime.CompilerServices;

namespace LabFusion.Extensions
{
    public static partial class ArrayExtensions
    {
        public static void EnsureLength<T>(ref T[] array, int length
#if DEBUG
            , [CallerLineNumber] int lineNumber = 0
#endif
            ) where T : struct
        {
            if (array.Length < length)
            {
                Array.Resize(ref array, length);

#if DEBUG
                if (FusionUnityLogger.EnableArrayResizeLogs)
#pragma warning disable CS0162 // Unreachable code detected
                    FusionLogger.Log($"An array was resized at line {lineNumber}. This action is costly.", ConsoleColor.DarkMagenta);
#pragma warning restore CS0162 // Unreachable code detected
#endif
            }
        }
    }
}
