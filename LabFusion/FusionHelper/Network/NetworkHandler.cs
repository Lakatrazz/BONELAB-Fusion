using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Steamworks;

using LiteNetLib;
using LiteNetLib.Utils;

using FusionHelper.Extensions;
using FusionHelper.Steamworks;
using System.Xml.Linq;

namespace FusionHelper.Network
{
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
                SteamHandler.Shutdown();

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
#elif PLATFORM_MAC
            Console.WriteLine("\x1b[93mBe sure to allow FusionHelper to receive and send information if/when the firewall asks.\x1b[0m");
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

        private static async Task<CSteamID[]> FetchLobbies()
        {
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            SteamMatchmaking.AddRequestLobbyListFilterSlotsAvailable(int.MaxValue);
            SteamMatchmaking.AddRequestLobbyListResultCountFilter(int.MaxValue);
            SteamMatchmaking.AddRequestLobbyListStringFilter("BONELAB_FUSION_HasServerOpen", bool.TrueString, ELobbyComparison.k_ELobbyComparisonEqual);

            var task = SteamMatchmaking.RequestLobbyList();

            CSteamID[]? lobbyIdList = null;
            var res = CallResult<LobbyMatchList_t>.Create((lml, good) =>
            {
                if (lml.m_nLobbiesMatching == 0)
                {
                    lobbyIdList = Array.Empty<CSteamID>();
                }

                lobbyIdList = new CSteamID[lml.m_nLobbiesMatching];

                for (int i = 0; i < lml.m_nLobbiesMatching; i++)
                {
                    lobbyIdList[i] = SteamMatchmaking.GetLobbyByIndex(i);
                }
            });
            res.Set(task);

            while (lobbyIdList == null)
            {
                await Task.Delay(250);
            }

            return lobbyIdList;
        }

        private static async void RespondWithLobbyIds()
        {
            CSteamID[] lobbies = await FetchLobbies();
            if (lobbies == null)
            {
                Console.WriteLine("Failed to fetch lobbies! Make sure your Steam client is connected to their servers and restart FusionHelper. If that doesn't solve it, Steam's servers may be down right now.");
                return;
            }

            NetDataWriter writer = new();
            writer.Put((byte)MessageTypes.LobbyIds);
            writer.Put((uint)lobbies.Length);
            
            foreach (CSteamID l in lobbies)
            {
                writer.Put(l.m_SteamID);
            }

            ClientConnection.Send(writer, DeliveryMethod.ReliableUnordered);
        }

        private static void RespondWithLobbyMetadata(ulong lobbyId)
        {
            CSteamID lobby = new(lobbyId);

            NetDataWriter writer = new();
            writer.Put((byte)MessageTypes.LobbyMetadata);

            // Lobby Id
            writer.Put(lobbyId);

            // Key collection, contains an array of all keys in the metadata
            string[] keyCollection =  SteamMatchmaking.GetLobbyData(lobby, "BONELAB_FUSION_KeyCollection").Expand();

            // Array length
            writer.Put(keyCollection.Length);

            // Write entire array
            for (var i = 0; i < keyCollection.Length; i++) {
                var key = keyCollection[i];
                var value = SteamMatchmaking.GetLobbyData(lobby, key);

                // In order, key then value
                writer.Put(key);
                writer.Put(value);
            }

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
                        ulong steamID = SteamUser.GetSteamID().m_SteamID;
                        NetDataWriter writer = NewWriter(MessageTypes.SteamID);
                        writer.Put(steamID);
                        SendToClient(writer);
                        break;
                    }

                case (ulong)MessageTypes.GetUsername:
                    {
                        ulong userId = dataReader.GetULong();
                        string name = SteamFriends.GetFriendPersonaName(new CSteamID(userId));
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

                        foreach (var connection in SteamHandler.SocketManager.ConnectedSteamIds)
                        {
                            SteamNetworkingSockets.SendMessageToConnection(connection.Value, intPtrMessage, (uint)sizeOfMessage, (int)sendType, out long _);
                        }

                        Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                        break;
                    }

                case (ulong)MessageTypes.ReliableBroadcastToServer:
                case (ulong)MessageTypes.UnreliableBroadcastToServer:
                    {
                        if (SteamHandler.ConnectionManager.Connection.m_HSteamNetConnection == 0)
                        {
                            Console.WriteLine("\x1b[91mFusion is trying to broadcast without a valid connection. Please restart BONELAB. \x1b[0m");
                            return;
                        }

                        byte[] data = dataReader.GetBytesWithLength();

                        // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
                        int sizeOfMessage = data.Length;
                        IntPtr intPtrMessage = Marshal.AllocHGlobal(sizeOfMessage);
                        Marshal.Copy(data, 0, intPtrMessage, sizeOfMessage);

                        SendType sendType = id == (ulong)MessageTypes.ReliableBroadcastToServer ? SendType.Reliable : SendType.Unreliable;

                        try
                        {
                            EResult success = SteamNetworkingSockets.SendMessageToConnection(SteamHandler.ConnectionManager.Connection, intPtrMessage, (uint)sizeOfMessage, (int)sendType, out long _);
                            if (success == EResult.k_EResultOK)
                            {
                                Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                            }
                            else
                            {
                                // RETRY
                                EResult retry = SteamNetworkingSockets.SendMessageToConnection(SteamHandler.ConnectionManager.Connection, intPtrMessage, (uint)sizeOfMessage, (int)sendType, out long _);
                                Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer

                                if (retry != EResult.k_EResultOK)
                                {
                                    throw new Exception($"Steam result was {retry}.");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
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
                        else if (userId == SteamUser.GetSteamID().m_SteamID)
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
                case (ulong)MessageTypes.SetLobbyMetadata:
                    {
                        SteamHandler.SetMetadata(dataReader.GetString(), dataReader.GetString());
                        break;
                    }
                case (ulong)MessageTypes.SteamFriends:
                    {
                        List<ulong> friendList = new List<ulong>();
                        for (int i = 0; i < SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate); i++)
                        {
                            CSteamID friendId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                            friendList.Add(friendId.m_SteamID);
                        }
                        NetDataWriter writer = NewWriter(MessageTypes.SteamFriends);
                        writer.PutArray(friendList.ToArray());
                        SendToClient(writer);
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
