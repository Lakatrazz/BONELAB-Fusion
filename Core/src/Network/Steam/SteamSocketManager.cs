using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Extensions;
using LabFusion.Utilities;

using Steamworks;
using Steamworks.Data;

namespace LabFusion.Network
{
    public class SteamSocketManager : SocketManager {
        public Dictionary<ulong, Connection> ConnectedSteamIds = new Dictionary<ulong, Connection>();

        public override void OnConnecting(Connection connection, ConnectionInfo data) {
            base.OnConnecting(connection, data);
            connection.Accept();
        }

        public override void OnConnected(Connection connection, ConnectionInfo data) {
            base.OnConnected(connection, data);
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo data) {
            base.OnDisconnected(connection, data);

            var pair = ConnectedSteamIds.First((p) => p.Value.Id == connection.Id);

#if DEBUG
            FusionLogger.Log($"Server received disconnect from long id {pair.Key}.");
#endif

            using (FusionWriter writer = FusionWriter.Create()) {
                var disconnect = new DisconnectMessageData() {
                    longId = pair.Key
                };

                using (var message = FusionMessage.Create(NativeMessageTag.Disconnect, writer)) {
                    this.BroadcastToClients(NetworkChannel.Reliable, message);
                }
            }

            ConnectedSteamIds.Remove(pair.Key);

            NetworkUtilities.RemoveUser(pair.Key);
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel) {
            base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);

            if (!ConnectedSteamIds.ContainsKey(identity.steamid))
                ConnectedSteamIds.Add(identity.steamid, connection);

            SteamSocketHandler.OnSocketMessageReceived(data, size);
        }
    }
}
