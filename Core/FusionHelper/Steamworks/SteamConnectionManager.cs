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
    public class SteamConnectionManager : ConnectionManager
    {
        public override void OnConnected(ConnectionInfo info)
        {
            base.OnConnected(info);
        }

        public override void OnConnecting(ConnectionInfo info)
        {
            base.OnConnecting(info);
        }

        public override void OnDisconnected(ConnectionInfo info)
        {
            base.OnDisconnected(info);

            NetworkHandler.SendToClient(MessageTypes.OnConnectionDisconnected);

#if DEBUG
            Console.WriteLine("Client was disconnected.");
#endif
        }

        public override void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            base.OnMessage(data, size, messageNum, recvTime, channel);
            byte[] message = new byte[size];
            Marshal.Copy(data, message, 0, size);

            NetDataWriter writer = NetworkHandler.NewWriter(MessageTypes.OnConnectionMessage);
            writer.PutBytesWithLength(message);
            NetworkHandler.SendToClient(writer);
        }
    }
}
