using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Utilities;

using Steamworks;
using Steamworks.Data;

namespace LabFusion.Network
{
    public class SteamConnectionManager : ConnectionManager
    {
        public override void OnConnected(ConnectionInfo info)
        {
            base.OnConnected(info);
            FusionLogger.Log("ConnectionOnConnected");
        }

        public override void OnConnecting(ConnectionInfo info)
        {
            base.OnConnecting(info);
            FusionLogger.Log("ConnectionOnConnecting");
        }

        public override void OnDisconnected(ConnectionInfo info)
        {
            base.OnDisconnected(info);
            FusionLogger.Log("ConnectionOnDisconnected");
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            FusionLogger.Log("Connection Got A Message");
        }
    }
}
