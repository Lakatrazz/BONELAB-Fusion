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
        
        [DllImport("libdl.so")]
        public static extern IntPtr dlopen(string filename, int flags = 2);

        [DllImport("libdl.so")]
        public static extern int dlclose(IntPtr handle);

        [DllImport("libdl.so")]
        public static extern IntPtr dlerror();
    }
}
