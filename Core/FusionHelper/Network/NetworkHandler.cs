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
            // TODO: disconnect timeout doesn't work?
            //Server.DisconnectTimeout = 10;
            listener.ConnectionRequestEvent += request =>
            {
                if (Server.ConnectedPeersCount < 1)
                    request.AcceptIfKey("ProxyConnection");
                else
                    request.Reject();
            };
            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine("New connected peer: {0}", peer.EndPoint);
                ClientConnection = peer;
            };
            listener.PeerDisconnectedEvent += (peer, disconnectInfo) => {
                Thread.CurrentThread.Join();
                Console.WriteLine("Client disconnected, press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            };
            listener.NetworkReceiveEvent += EvaluateMessage;

            Server.Start(9000);

            Console.WriteLine("Initialized UDP socket at localhost:9000");
        }

        private static void EvaluateMessage(NetPeer fromPeer, NetPacketReader dataReader, byte channel, DeliveryMethod deliveryMethod)
        {
            ulong id = dataReader.GetByte();
            switch (id)
            {
                case (ulong)MessageTypes.Ping:
                    double theTime = dataReader.GetDouble();
                    double curTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                    Console.WriteLine("Client -> Server = " + (curTime - theTime) + " ms.");
                    break;
                case (ulong)MessageTypes.SteamID:
                    {                     
                        ulong steamID = SteamClient.IsValid ? SteamClient.SteamId : 0;
                        NetDataWriter writer = NewWriter(MessageTypes.SteamID);
                        writer.Put(steamID);
                        SendToClient(writer);
                        break;
                    }

                case (ulong)MessageTypes.GetUsername:
                    {
                        ulong userId = dataReader.GetULong();
                        string name = new Friend(userId).Name;
                        NetDataWriter writer = NewWriter(MessageTypes.GetUsername);
                        writer.Put(name);
                        SendToClient(writer);
                    }
                    break;

                case (ulong)MessageTypes.ReliableBroadcastToClients:
                case (ulong)MessageTypes.UnreliableBroadcastToClients:
                    {
                        byte[] data = dataReader.GetBytesWithLength();

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
                        byte[] data = dataReader.GetBytesWithLength();

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
                case (ulong)MessageTypes.UnreliableSendFromServer:
                case (ulong)MessageTypes.ReliableSendFromServer:
                    {
                        ulong userId = dataReader.GetULong();
                        byte[] message = dataReader.GetBytesWithLength();
                        bool reliable = id != (ulong)MessageTypes.UnreliableSendFromServer;

                        if (SteamHandler.SocketManager.ConnectedSteamIds.ContainsKey(userId))
                            SteamHandler.SendToClient(SteamHandler.SocketManager.ConnectedSteamIds[userId], message, reliable);
                        else if (userId == SteamClient.SteamId)
                            SteamHandler.SendToClient(SteamHandler.ConnectionManager.Connection, message, reliable);

                        break;
                    }
                case (ulong)MessageTypes.JoinServer:
                    {
                        ulong serverId = dataReader.GetULong();
                        SteamHandler.ConnectRelay(serverId);
                    }
                    break;
                case (ulong)MessageTypes.StartServer:
                    SteamHandler.CreateRelay();
                    SendToClient(MessageTypes.StartServer);
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

        public static NetDataWriter NewWriter(MessageTypes type)
        {
            NetDataWriter writer = new();
            writer.Put((byte)type);
            return writer;
        }

        public static void SendToClient(NetDataWriter writer)
        {
            ClientConnection.Send(writer, DeliveryMethod.Unreliable);
        }

        public static void SendToClient(MessageTypes type)
        {
            NetDataWriter writer = NewWriter(type);
            ClientConnection.Send(writer, DeliveryMethod.Unreliable);
        }
    }
}
