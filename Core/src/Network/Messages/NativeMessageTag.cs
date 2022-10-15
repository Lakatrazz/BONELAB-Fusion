using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public static class NativeMessageTag
    {
        public static byte
            Unknown = 0,

            ConnectionRequest = 1,
            ConnectionResponse = 2,
            Disconnect = 3,

            PlayerRepTransform = 4,
            PlayerRepAvatar = 5,
            PlayerRepVitals = 6,

            PlayerRepGrab = 7,
            PlayerRepRelease = 8,
            PlayerRepAnchors = 9,

            SceneLoad = 10,

            ModuleMessage = 80;
    }

}
