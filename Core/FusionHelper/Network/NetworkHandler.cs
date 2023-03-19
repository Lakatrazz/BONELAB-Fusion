using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ruffles.Channeling;
using Ruffles.Configuration;
using Ruffles.Connections;
using Ruffles.Core;
using FusionHelper.Network;
using FusionHelper.Steamworks;
using System.Runtime.InteropServices;
using Steamworks.Data;

namespace FusionHelper.WebSocket
{
    internal static class NetworkHandler
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static RuffleSocket Server { get; private set; }
        public static Ruffles.Connections.Connection ClientConnection { get; private set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public static void Init()
        {
            Server = new RuffleSocket(new SocketConfig()
            {
                ChallengeDifficulty = 20, // Difficulty 20 is fairly hard
                ChannelTypes = new ChannelType[]
                {
                    ChannelType.Reliable,
                    ChannelType.Unreliable,
                },
                DualListenPort = 9000,
            });
            Server.Start();

            Console.WriteLine("Initialized UDP socket at localhost:9000");
        }

        public static void PollEvents()
        {
            NetworkEvent serverEvent = Server.Poll();

            if (serverEvent.Type != NetworkEventType.Nothing)
            {
                if (serverEvent.Type == NetworkEventType.Connect)
                {
                    ClientConnection = serverEvent.Connection;
                    Console.WriteLine("Client was connected");
                }

                if (serverEvent.Type == NetworkEventType.Data)
                {
                    ulong id = serverEvent.Data.Last();
                    byte[] data = serverEvent.Data.SkipLast(1).ToArray();

                    switch (id)
                    {
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
                }
            }

            serverEvent.Recycle();
        }

        public static void SendToClient(byte[] data, MessageTypes message)
        {
            var a = data.ToList();
            a.Add((byte)message);
            ClientConnection.Send(new ArraySegment<byte>(a.ToArray()), 1, false, 0);
        }
    }
}
