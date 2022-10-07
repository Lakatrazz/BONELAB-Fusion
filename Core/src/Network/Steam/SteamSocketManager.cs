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
    public class SteamSocketManager : SocketManager {
        public override void OnConnecting(Connection connection, ConnectionInfo data) {
            connection.Accept();
            FusionLogger.Log($"{data.Identity} is connecting");
        }

        public override void OnConnected(Connection connection, ConnectionInfo data) {
            FusionLogger.Log($"{data.Identity} has joined the game");
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo data) {
            FusionLogger.Log($"{data.Identity} is out of here");
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel) {
            FusionLogger.Log($"We got a message from {identity}!");

            // Send it right back
            connection.SendMessage(data, size, SendType.Reliable);
        }
    }
}
