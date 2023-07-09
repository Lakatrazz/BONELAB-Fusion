using System;
using System.Runtime.InteropServices;

//https://stackoverflow.com/questions/16518943/dllimport-or-loadlibrary-for-best-performance
//https://newbedev.com/how-can-i-specify-a-dllimport-path-at-runtime
namespace LabFusion.Utilities
{
    public static class DllTools
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string lpLibFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern UInt32 GetLastError();
    }
}
