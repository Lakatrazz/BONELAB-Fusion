using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionHelper.Network
{
    internal enum MessageTypes
    {
        SteamID = 0,
        OnDisconnected = 1,
        OnMessage = 2,
        GetUsername = 3,
    }
}
