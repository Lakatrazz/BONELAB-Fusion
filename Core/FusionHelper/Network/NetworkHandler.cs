using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FusionHelper.Steamworks;
using System.Runtime.InteropServices;
using Steamworks.Data;
using LiteNetLib;
using LiteNetLib.Utils;

namespace FusionHelper.Network
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

        private static bool _hasBeenDiscovered;

        public static void Init()
        {
            EventBasedNetListener listener = new();
            Server = new(listener)
            {
                UnconnectedMessagesEnabled = true,
                BroadcastReceiveEnabled = true,
                PingInterval = 1000,
            };
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
                Console.WriteLine("Client disconnected, resetting for reuse.");
                Server.DisconnectPeerForce(peer);
                _hasBeenDiscovered = false;
            };
            listener.NetworkReceiveEvent += EvaluateMessage;
            listener.NetworkReceiveUnconnectedEvent += (endPoint, reader, messageType) =>
            {
                if (_hasBeenDiscovered) return;

                if (reader.TryGetString(out string data) && data == "FUSION_SERVER_DISCOVERY")
                {
                    Console.WriteLine("Client has found the server, letting it know.");
                    NetDataWriter writer = new();
                    writer.Put("YOU_FOUND_ME");
                    Server.SendUnconnectedMessage(writer, endPoint);
                    _hasBeenDiscovered = true;
                }
            };

            Server.Start(ReadPort());

            Console.WriteLine("Initialized UDP socket on port " + Server.LocalPort);
#if PLATFORM_WIN
            Console.WriteLine("\x1b[93mBe sure to allow FusionHelper access both public and private networks if/when the firewall asks.\x1b[0m");
#endif
        }

        private static int ReadPort()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "port.txt");
            if (File.Exists(path))
            {
                string data = File.ReadAllText(path);
                if (int.TryParse(data, out int port) && port >= 1024 && port <= 65535)
                    return port;
                else
                    Console.WriteLine("Custom port is invalid, using default!");
            }

            return 28340;
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
            if (lobbies == null)
            {
                Console.WriteLine("Failed to fetch lobbies! Make sure your Steam client is connected to their servers and restart FusionHelper. If that doesn't solve it, Steam's servers may be down right now.");
                return;
            }

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

            // Lobby Id
            writer.Put(lobbyId);

            // Metadata Lobby Id
            ulong.TryParse(lobby.GetData("LobbyId"), out ulong metaLobbyId);
            writer.Put(metaLobbyId);

            // Lobby Owner
            writer.Put(lobby.GetData("LobbyOwner"));

            // Lobby Name
            string name = lobby.GetData("LobbyName");
            Console.WriteLine($"Writing metadata for {name} (id {lobbyId})");
            writer.Put(name);

            // Open Status
            bool hasServerOpen = lobby.GetData("BONELAB_FUSION_HasServerOpen") == bool.TrueString;
            Console.WriteLine($"Has Server Open: {hasServerOpen}");
            writer.Put(hasServerOpen);

            // Player Count
            int.TryParse(lobby.GetData("PlayerCount"), out int playerCount);
            writer.Put(playerCount);

            // Nametag Settings
            writer.Put(lobby.GetData("NametagsEnabled") == bool.TrueString);

            // Privacy Settings
            Enum.TryParse(lobby.GetData("Privacy"), out ServerPrivacy privacy);
            writer.Put((int)privacy);

            // TimeScale Settings
            Enum.TryParse(lobby.GetData("TimeScaleMode"), out TimeScaleMode tsMode);
            writer.Put((int)tsMode);

            // Max Players
            int.TryParse(lobby.GetData("MaxPlayers"), out int maxPlayers);
            writer.Put(maxPlayers);

            // Voicechat Settings
            writer.Put(lobby.GetData("VoicechatEnabled") == bool.TrueString);

            // Level and Gamemode
            writer.Put(lobby.GetData("LevelName"));
            writer.Put(lobby.GetData("LevelBarcode"));
            writer.Put(lobby.GetData("GamemodeName"));

            // Player List
            writer.Put(lobby.GetData("PlayerList"));

            // Put LobbyVersion at the end because of weird stuff in the deserialisation code
            writer.Put(lobby.GetData("LobbyVersion"));

            ClientConnection.Send(writer, DeliveryMethod.ReliableUnordered);
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
                        // Initialize networking
                        int appId = dataReader.GetInt();
                        SteamHandler.Init(appId);

                        // Actually send back SteamID
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
                case (ulong)MessageTypes.LobbyIds:
                    RespondWithLobbyIds();
                    break;
                case (ulong)MessageTypes.LobbyMetadata:
                    RespondWithLobbyMetadata(dataReader.GetULong());
                    break;
                case (ulong)MessageTypes.UpdateConnectPresence:
                    {
                        string? data = dataReader.GetString() == "true" ? "true" : null;
                        SteamFriends.SetRichPresence("connect", data);
                        break;
                    }
                case (ulong)MessageTypes.DecompressVoice:
                    {
                        ulong playerId = dataReader.GetULong();
                        byte[] audioData = dataReader.GetBytesWithLength();
                        byte[] decompressed = SteamHandler.DecompressVoice(audioData);
                        NetDataWriter writer = NewWriter(MessageTypes.DecompressVoice);
                        writer.Put(playerId);
                        writer.PutBytesWithLength(decompressed);
                        SendToClient(writer);
                        break;
                    }
                case (ulong)MessageTypes.SetLobbyMetadata:
                    {
                        SteamHandler.SetMetadata(dataReader.GetString(), dataReader.GetString());
                        break;
                    }
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
            ClientConnection.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public static void SendToClient(MessageTypes type)
        {
            NetDataWriter writer = NewWriter(type);
            ClientConnection.Send(writer, DeliveryMethod.ReliableOrdered);
        }
    }
}
