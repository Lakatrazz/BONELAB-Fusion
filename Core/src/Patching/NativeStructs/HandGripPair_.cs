using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.NativeStructs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct HandGripPair_
    {
        public IntPtr hand;

        public IntPtr grip;
    }
}
