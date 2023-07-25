using Steamworks.Data;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FusionHelper.Network;
using System.Runtime.InteropServices;
using LiteNetLib.Utils;

namespace FusionHelper.Steamworks
{
    public class SteamSocketManager : SocketManager
    {
        public Dictionary<ulong, Connection> ConnectedSteamIds = new Dictionary<ulong, Connection>();

        public override void OnConnecting(Connection connection, ConnectionInfo data)
        {
            base.OnConnecting(connection, data);
            connection.Accept();
        }

        public override void OnConnected(Connection connection, ConnectionInfo data)
        {
            base.OnConnected(connection, data);
        }

        public override void OnDisconnected(Connection connection, ConnectionInfo data)
        {
            base.OnDisconnected(connection, data);

            var pair = ConnectedSteamIds.FirstOrDefault((p) => p.Value.Id == connection.Id);
            var longId = pair.Key;

            ConnectedSteamIds.Remove(longId);

            NetDataWriter writer = NetworkHandler.NewWriter(MessageTypes.OnDisconnected);
            writer.Put(longId);
            NetworkHandler.SendToClient(writer);
        }

        public override void OnMessage(Connection connection, NetIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            base.OnMessage(connection, identity, data, size, messageNum, recvTime, channel);

            if (!ConnectedSteamIds.ContainsKey(identity.SteamId))
                ConnectedSteamIds.Add(identity.SteamId, connection);

            byte[] message = new byte[size];
            Marshal.Copy(data, message, 0, size);

            NetDataWriter writer = NetworkHandler.NewWriter(MessageTypes.OnMessage);
            writer.PutBytesWithLength(message);
            NetworkHandler.SendToClient(writer);
        }
    }
}
