using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Patching
{
    public delegate void ReceiveAttackPatchDelegate(IntPtr instance, IntPtr attack, IntPtr method);

    public delegate void EmptyPatchDelegate(IntPtr instance, IntPtr method);
}
