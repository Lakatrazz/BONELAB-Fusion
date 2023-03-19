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
    public enum ServerPrivacy
    {
        PUBLIC = 0,
        PRIVATE = 1,
        FRIENDS_ONLY = 2,
        LOCKED = 3,
    }

    public enum TimeScaleMode
    {
        DISABLED = 0,
        LOW_GRAVITY = 1,
        HOST_ONLY = 2,
        EVERYONE = 3,
        CLIENT_SIDE_UNSTABLE = 4,
    }

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

        private static Task<Lobby[]> FetchLobbies()
        {
            var list = SteamMatchmaking.LobbyList;
            list.FilterDistanceWorldwide();
            list.WithMaxResults(int.MaxValue);
            list.WithSlotsAvailable(int.MaxValue);
            list.WithKeyValue("BONELAB_FUSION_HasServerOpen", bool.TrueString);
            return list.RequestAsync();
        }

        private static async void RespondWithLobbyIds()
        {
            Lobby[] lobbies = await FetchLobbies();
            NetDataWriter writer = new();
            writer.Put((byte)MessageTypes.LobbyIds);
            writer.Put((uint)lobbies.Length);
            
            foreach (Lobby l in lobbies)
            {
                writer.Put(l.Id.Value);
            }

            ClientConnection.Send(writer, DeliveryMethod.ReliableUnordered);
        }

        private static void RespondWithLobbyMetadata(ulong lobbyId)
        {
            Lobby lobby = new(lobbyId);

            // oh dear.
            NetDataWriter writer = new();
            writer.Put((byte)MessageTypes.LobbyMetadata);
            writer.Put(lobbyId);
            ulong.TryParse(lobby.GetData("LobbyId"), out ulong metaLobbyId);

            writer.Put(metaLobbyId);
            string name = lobby.GetData("LobbyName");
            Console.WriteLine($"Writing metadata for {name} (id {lobbyId})");
            writer.Put(name);

            bool hasServerOpen = lobby.GetData("BONELAB_FUSION_HasServerOpen") == bool.TrueString;
            Console.WriteLine($"Has Server Open: {hasServerOpen}");
            writer.Put(hasServerOpen);

            int.TryParse(lobby.GetData("PlayerCount"), out int playerCount);

            writer.Put(playerCount);

            writer.Put(lobby.GetData("NametagsEnabled") == bool.TrueString);

            Enum.TryParse(lobby.GetData("Privacy"), out ServerPrivacy privacy);
            writer.Put((int)privacy);

            Enum.TryParse(lobby.GetData("TimeScaleMode"), out TimeScaleMode tsMode);
            writer.Put((int)tsMode);

            int.TryParse(lobby.GetData("MaxPlayers"), out int maxPlayers);
            writer.Put(maxPlayers);

            writer.Put(lobby.GetData("VoicechatEnabled") == bool.TrueString);

            writer.Put(lobby.GetData("LevelName"));
            writer.Put(lobby.GetData("GamemodeName"));

            // Put LobbyVersion at the end because of weird stuff in the deserialisation code
            writer.Put(lobby.GetData("LobbyVersion"));

            ClientConnection.Send(writer, DeliveryMethod.ReliableUnordered);
        }

        private static void EvaluateMessage(NetPeer fromPeer, NetPacketReader dataReader, byte channel, DeliveryMethod deliveryMethod)
        {
            ulong id = dataReader.GetByte();

            if (id == (ulong)MessageTypes.LobbyMetadata)
            {
                RespondWithLobbyMetadata(dataReader.GetULong());
                dataReader.Recycle();
                return;
            }

            if (id == (ulong)MessageTypes.LobbyOwner)
            {
                ulong lobbyId = dataReader.GetULong();
                Lobby lobby = new(lobbyId);

                NetDataWriter writer = new();
                writer.Put((byte)MessageTypes.LobbyOwner);
                writer.Put(lobbyId);
                writer.Put(lobby.Owner.Id.Value);

                Console.WriteLine($"Owner of {lobbyId} is {lobby.Owner.Id.Value}");
                ClientConnection.Send(writer, DeliveryMethod.ReliableUnordered);
                dataReader.Recycle();
                return;
            }

            byte[] data = dataReader.GetBytesWithLength();
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
                case (ulong)MessageTypes.LobbyIds:
                    RespondWithLobbyIds();
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
            writer.PutBytesWithLength(data, 0, (ushort)data.Length);
            ClientConnection.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }
}
