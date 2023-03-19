using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FusionHelper.Network;
using FusionHelper.Steamworks;
using System.Runtime.InteropServices;
using Steamworks.Data;
using LiteNetLib;
using LiteNetLib.Utils;

namespace FusionHelper.WebSocket
{
    internal static class NetworkHandler
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static NetManager Server { get; private set; }
        public static NetPeer ClientConnection { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static void Init()
        {
            EventBasedNetListener listener = new();
            Server = new(listener);
            listener.ConnectionRequestEvent += request =>
            {
                if (Server.ConnectedPeersCount < 1)
                    request.AcceptIfKey("ProxyConnection");
                else
                    request.Reject();
            };
            listener.NetworkReceiveEvent += EvaluateMessage;

            Server.Start(9000);

            Console.WriteLine("Initialized UDP socket at localhost:9000");
        }

        private static void EvaluateMessage(NetPeer fromPeer, NetPacketReader dataReader, byte channel, DeliveryMethod deliveryMethod)
        {
            Console.WriteLine("1. " + dataReader.AvailableBytes);
            ulong id = dataReader.GetByte();
            Console.WriteLine("2. " + dataReader.AvailableBytes);
            byte[] data = dataReader.GetRemainingBytes();
            switch (id)
            {
                case (ulong)MessageTypes.Ping:
                    double theTime = BitConverter.ToDouble(data, 0);
                    double curTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                    Console.WriteLine("Client -> Server = " + (curTime - theTime) + " ms.");
                    break;
                case (ulong)MessageTypes.SteamID:
                    ulong steamID = SteamClient.IsValid ? SteamClient.SteamId : 0;
                    SendToClient(BitConverter.GetBytes(steamID), (ulong)MessageTypes.SteamID);
                    break;

                case (ulong)MessageTypes.GetUsername:
                    SendToClient(Encoding.UTF8.GetBytes(new Friend(BitConverter.ToUInt64(data)).Name), MessageTypes.GetUsername);
                    break;

                case (ulong)MessageTypes.ReliableBroadcastToClients:
                case (ulong)MessageTypes.UnreliableBroadcastToClients:
                    {
                        // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
                        int sizeOfMessage = data.Length;
                        IntPtr intPtrMessage = Marshal.AllocHGlobal(sizeOfMessage);
                        Marshal.Copy(data, 0, intPtrMessage, sizeOfMessage);

                        SendType sendType = id == (ulong)MessageTypes.ReliableBroadcastToClients ? SendType.Reliable : SendType.Unreliable;

                        for (var i = 0; i < SteamHandler.SocketManager.Connected.Count; i++)
                        {
                            var connection = SteamHandler.SocketManager.Connected[i];
                            connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);
                        }

                        Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                        break;
                    }

                case (ulong)MessageTypes.ReliableBroadcastToServer:
                case (ulong)MessageTypes.UnreliableBroadcastToServer:
                    {
                        // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
                        int sizeOfMessage = data.Length;
                        IntPtr intPtrMessage = Marshal.AllocHGlobal(sizeOfMessage);
                        Marshal.Copy(data, 0, intPtrMessage, sizeOfMessage);

                        SendType sendType = id == (ulong)MessageTypes.ReliableBroadcastToServer ? SendType.Reliable : SendType.Unreliable;

                        Result success = SteamHandler.ConnectionManager.Connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);
                        if (success == Result.OK)
                        {
                            Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                        }
                        else
                        {
                            // RETRY
                            Result retry = SteamHandler.ConnectionManager.Connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);
                            Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer

                            if (retry != Result.OK)
                            {
                                throw new Exception($"Steam result was {retry}.");
                            }
                        }
                        break;
                    }
                case (ulong)MessageTypes.JoinServer:
                    ulong serverId = BitConverter.ToUInt64(data, 0);
                    SteamHandler.ConnectRelay(serverId);
                    break;
                case (ulong)MessageTypes.Disconnect:
                    SteamHandler.KillConnection();
                    break;
            }

            dataReader.Recycle();
        }

        public static void PollEvents()
        {
            Server.PollEvents();
        }

        public static void SendToClient(byte[] data, MessageTypes message)
        {
            NetDataWriter writer = new();
            writer.Put((byte)message);
            writer.PutArray(data, data.Length);
            ClientConnection.Send(writer, DeliveryMethod.Unreliable);
        }
    }
}
