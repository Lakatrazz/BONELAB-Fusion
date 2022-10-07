using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public class NativeMessageTag
    {
        public static byte
            Unknown = 0,

            ConnectionRequest = 1,
            ConnectionResponse = 2,
            Disconnect = 3,

            ModuleMessage = 80;
    }

}
