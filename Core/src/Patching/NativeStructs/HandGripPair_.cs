using System;
using System.Runtime.InteropServices;

namespace LabFusion.NativeStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HandGripPair_
    {
        public IntPtr hand;

        public IntPtr grip;
    }
}
