using HarmonyLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Utilities {
    internal static unsafe class NativeUtilities {
        internal static IntPtr GetNativePtr<T>(string name) {
            return *(IntPtr*)(IntPtr)typeof(T).GetField(name, AccessTools.all).GetValue(null);
        }
    }
}
