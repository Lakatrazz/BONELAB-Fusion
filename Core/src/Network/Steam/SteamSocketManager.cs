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
            base.OnConnecting(connection, data);
            connection.Accept();
            FusionLogger.Log($"{data.Identity.SteamId} is connecting");
        }

        public override void OnConnected(Connection connection, ConnectionInfo data) {
            base.OnConnected(connection, data);
            FusionLogger.Log($"{data.Identity.SteamId} has joined the game");
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo data) {
            base.OnDisconnected(connection, data);
            FusionLogger.Log($"{data.Identity.SteamId} is out of here");
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel) {
            base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);
            FusionLogger.Log($"We got a message from {identity.SteamId}!");

            SteamSocketHandler.OnSocketMessageReceived(data, size);
        }
    }
}
