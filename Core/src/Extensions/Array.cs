#if DEBUG
using LabFusion.Debugging;
#endif

using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Extensions
{
    public static partial class ArrayExtensions {
        public static void EnsureLength<T>(ref T[] array, int length) where T : struct {
            if (array.Length < length) {
                Array.Resize(ref array, length);

#if DEBUG
                if (FusionUnityLogger.EnableArrayResizeLogs)
#pragma warning disable CS0162 // Unreachable code detected
                    FusionLogger.Warn("An array was resized. This action is costly.");
#pragma warning restore CS0162 // Unreachable code detected
#endif
            }
        }
    }
}
