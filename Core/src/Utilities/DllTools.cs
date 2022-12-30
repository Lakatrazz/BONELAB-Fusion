using System;
using System.Runtime.InteropServices;

//https://stackoverflow.com/questions/16518943/dllimport-or-loadlibrary-for-best-performance
//https://newbedev.com/how-can-i-specify-a-dllimport-path-at-runtime
namespace LabFusion.Utilities
{
    public static class DllTools
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("Kernel32.dll")]
        public static extern UInt32 GetLastError();

        public static T GetFunction<T>(string signature, IntPtr hModule) where T : Delegate
        {
            if (hModule == IntPtr.Zero) throw new ArgumentException("hModule was a nullptr!");

            IntPtr procAddress = GetProcAddress(hModule, signature);
            return Marshal.GetDelegateForFunctionPointer<T>(procAddress);
        }
    }
}
