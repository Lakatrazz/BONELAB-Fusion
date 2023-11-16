using HarmonyLib;

using System;
using System.Runtime.InteropServices;

namespace LabFusion.Utilities
{
    internal static unsafe class NativeUtilities
    {
        internal static IntPtr GetNativePtr<T>(string name)
        {
            return *(IntPtr*)(IntPtr)typeof(T).GetField(name, AccessTools.all).GetValue(null);
        }

        internal static IntPtr GetDestPtr<TDelegate>(TDelegate destination) where TDelegate : Delegate
        {
            return destination.Method.MethodHandle.GetFunctionPointer();
        }

        internal static TDelegate GetOriginal<TDelegate>(IntPtr nativePtr)
        {
            return Marshal.GetDelegateForFunctionPointer<TDelegate>(nativePtr);
        }
    }
}
