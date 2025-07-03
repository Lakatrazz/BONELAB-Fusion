using System.Collections;

using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Preferences.Client;
using LabFusion.Voice;
using LabFusion.Voice.Unity;
using LabFusion.UI.Popups;

using MelonLoader;

using LabFusion.Senders;

using Steamworks;

using LiteNetLib;
using LiteNetLib.Utils;

namespace LabFusion.Network.Proxy;

public abstract class ProxyNetworkLayer : NetworkLayer
{
    public abstract uint ApplicationID { get; }

    public static ProxyNetworkLayer Instance { get; private set; }

    public override string Title => "Proxy";

    public override bool IsHost => _isServerActive;
    public override bool IsClient => _isConnectionActive;

    public override bool RequiresValidId => false;

    public SteamId SteamId;

    private INetworkLobby _currentLobby;
    public override INetworkLobby Lobby => _currentLobby;

    private IVoiceManager _voiceManager;
    public override IVoiceManager VoiceManager => _voiceManager;

    private IMatchmaker _matchmaker = null;
    public override IMatchmaker Matchmaker => _matchmaker;

    protected bool _isServerActive = false;
    protected bool _isConnectionActive = false;

    protected ulong _targetServerId;

    protected string _targetJoinId;

    protected bool _isInitialized = false;

    private NetManager client;
    private NetPeer serverConnection;
    private ProxyLobbyManager _lobbyManager;

    public override bool CheckSupported()
    {
        return PlatformHelper.IsAndroid;
    }

    public override bool CheckValidation()
    {
        return true;
    }

    public override void OnInitializeLayer()
    {
        Instance = this;

        _voiceManager = new UnityVoiceManager();
        _voiceManager.Enable();

        HookSteamEvents();

        _lobbyManager = new ProxyLobbyManager(this);

        _matchmaker = new ProxyMatchmaker(_lobbyManager);
    }

    public IEnumerator DiscoverServer()
    {
        int port = ClientSettings.ProxyPort.Value;

        if (!(port >= 1024 && port <= 65535))
        {
            FusionLogger.Error("Custom port is invalid, using default! (28430)");
            port = 28340;
        }

        float timeElapsed;

        NetDataWriter writer = new();
        writer.Put("FUSION_SERVER_DISCOVERY");

        while (serverConnection == null)
        {
            timeElapsed = 0;
            client.SendBroadcast(writer, port);

            // Wait every 5 seconds to try again incase it failed
            while (timeElapsed < 5f)
            {
                // Poll events while discovering
                client.PollEvents();

                // Tick time
                timeElapsed += TimeUtilities.DeltaTime;
                yield return null;
            }
        }
    }

    public void EvaluateMessage(NetPeer fromPeer, NetPacketReader dataReader, byte channel, DeliveryMethod deliveryMethod)
    {
        ulong id = dataReader.GetByte();
        switch (id)
        {
            case (ulong)MessageTypes.Ping:
                {
                    double theTime = dataReader.GetDouble();
                    double curTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
                    FusionLogger.Log("Server -> Client = " + (curTime - theTime) + " ms.");
                    NetDataWriter writer = NewWriter(MessageTypes.Ping);
                    writer.Put(curTime);
                    SendToProxyServer(writer);
                    break;
                }
            case (ulong)MessageTypes.SteamID:
                {
                    SteamId = new SteamId()
                    {
                        Value = dataReader.GetULong()
                    };

                    if (SteamId.Value == 0)
                    {
                        FusionLogger.Error("Steamworks failed to initialize!");
                        break;
                    }

                    PlayerIDManager.SetLongID(SteamId.Value);
                    NetDataWriter writer = NewWriter(MessageTypes.GetUsername);
                    writer.Put(SteamId.Value);
                    SendToProxyServer(writer);

                    FusionLogger.Log($"Steamworks initialized with SteamID {SteamId}!");

                    _isInitialized = true;
                    break;
                }
            case (ulong)MessageTypes.GetUsername:
                {
                    string username = dataReader.GetString();
                    LocalPlayer.Username = username;
                }
                break;
            case (ulong)MessageTypes.OnDisconnected:
                ulong longId = dataReader.GetULong();
                if (PlayerIDManager.HasPlayerID(longId))
                {
                    // Update the mod so it knows this user has left
                    InternalServerHelpers.OnPlayerLeft(longId);

                    // Send disconnect notif to everyone
                    ConnectionSender.SendDisconnect(longId);
                }
                break;
            case (ulong)MessageTypes.OnMessage:
                {
                    byte[] data = dataReader.GetBytesWithLength();
                    ProxySocketHandler.OnSocketMessageReceived(data, true);
                    break;
                }
            case (ulong)MessageTypes.OnConnectionDisconnected:
                NetworkHelper.Disconnect();
                break;
            case (ulong)MessageTypes.OnConnectionMessage:
                {
                    byte[] data = dataReader.GetBytesWithLength();
                    ProxySocketHandler.OnSocketMessageReceived(data, false);
                    break;
                }
            case (ulong)MessageTypes.JoinServer:
                {
                    ulong serverId = dataReader.GetULong();
                    JoinServer(new SteamId()
                    {
                        Value = serverId
                    });
                }
                break;
            case (ulong)MessageTypes.StartServer:
                {
                    _isServerActive = true;
                    _isConnectionActive = true;

                    // Call server setup
                    InternalServerHelpers.OnStartServer();

                    RefreshServerCode();
                    break;
                }
            case (ulong)MessageTypes.LobbyIds:
            case (ulong)MessageTypes.LobbyMetadata:
                {
                    _lobbyManager.HandleLobbyMessage((MessageTypes)id, dataReader);
                    break;
                }
            case (ulong)MessageTypes.SteamFriends:
                {
                    FriendIds = dataReader.GetULongArray().ToList();
                    break;
                }
        }

        dataReader.Recycle();
    }

    public override void OnDeinitializeLayer()
    {
        Disconnect();

        UnHookSteamEvents();

        _voiceManager.Disable();
        _voiceManager = null;
    }

    public override void LogIn()
    {
        // If a client is currently running, cancel it
        if (client != null)
        {
            client.Stop();

            client = null;
            serverConnection = null;
        }

        EventBasedNetListener listener = new();
        client = new NetManager(listener)
        {
            UnconnectedMessagesEnabled = true,
            BroadcastReceiveEnabled = true,
            DisconnectOnUnreachable = true,
            DisconnectTimeout = 10000,
            PingInterval = 5000,
        };
        listener.NetworkReceiveEvent += EvaluateMessage;
        listener.PeerConnectedEvent += (peer) =>
        {
            InvokeLoggedInEvent();

            serverConnection = peer;
            NetDataWriter writer = NewWriter(MessageTypes.SteamID);

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                FusionLogger.Error("Proxy has disconnected, logging out!");
                serverConnection = null;

                InvokeLoggedOutEvent();
            };

            writer.Put(ApplicationID);
            SendToProxyServer(writer);
        };

        listener.NetworkReceiveUnconnectedEvent += (endPoint, reader, messageType) =>
        {
            if (reader.TryGetString(out string data) && data == "YOU_FOUND_ME")
            {
                FusionLogger.Log("Found the proxy server!");
                client.Connect(endPoint, "ProxyConnection");
            }

            reader.Recycle();
        };

        client.Start();
        FusionLogger.Log("Beginning proxy discovery...");
        MelonCoroutines.Start(DiscoverServer());
    }

    public override void LogOut()
    {
        // End the running client
        client.Stop();

        client = null;
        serverConnection = null;

        InvokeLoggedOutEvent();
    }

    public override void OnUpdateLayer()
    {
        client.PollEvents();
    }

    internal static NetDataWriter NewWriter(MessageTypes type)
    {
        NetDataWriter writer = new();
        writer.Put((byte)type);
        return writer;
    }

    public void SendToProxyServer(NetDataWriter writer)
    {
        if (serverConnection == null)
        {
            FusionLogger.Warn("Attempting to send data to a null server peer! Is the proxy active?");
            Notifier.Send(new Notification()
            {
                SaveToMenu = false,
                ShowPopup = true,
                PopupLength = 4,
                Title = "Connection Failed",
                Message = "Failed to send data to the proxy, is FusionHelper running on your computer?",
                Type = NotificationType.ERROR
            });
            return;
        }

        serverConnection.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    internal void SendToProxyServer(MessageTypes type)
    {
        if (serverConnection == null)
        {
            FusionLogger.Warn("Attempting to send data to a null server peer! Is the proxy active?");
            return;
        }

        NetDataWriter writer = NewWriter(type);
        serverConnection.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public static List<ulong> FriendIds = new();
    public override bool IsFriend(ulong userId)
    {
        if (FriendIds.Contains(userId))
            return true;
        else
            return false;
    }

    public override void BroadcastMessage(NetworkChannel channel, NetMessage message)
    {
        if (IsHost)
        {
            ProxySocketHandler.BroadcastToClients(channel, message);
        }
        else
        {
            ProxySocketHandler.BroadcastToServer(channel, message);
        }
    }

    public override void SendToServer(NetworkChannel channel, NetMessage message)
    {
        ProxySocketHandler.BroadcastToServer(channel, message);
    }

    public override void SendFromServer(byte userId, NetworkChannel channel, NetMessage message)
    {
        var id = PlayerIDManager.GetPlayerID(userId);

        if (id != null)
        {
            SendFromServer(id.PlatformID, channel, message);
        }
    }

    public override void SendFromServer(ulong userId, NetworkChannel channel, NetMessage message)
    {
        if (!IsHost)
        {
            return;
        }

        MessageTypes type = channel == NetworkChannel.Unreliable ? MessageTypes.UnreliableSendFromServer : MessageTypes.ReliableSendFromServer;
        NetDataWriter writer = NewWriter(type);
        writer.Put(userId);
        byte[] data = message.ToByteArray();
        writer.PutBytesWithLength(data);
        SendToProxyServer(writer);
    }

    public override void StartServer()
    {
        SendToProxyServer(MessageTypes.StartServer);
    }

    public void JoinServer(SteamId serverId)
    {
        // Leave existing server
        if (_isConnectionActive || _isServerActive)
            Disconnect();

        NetDataWriter writer = NewWriter(MessageTypes.JoinServer);
        writer.Put(serverId);
        SendToProxyServer(writer);

        _isServerActive = false;
        _isConnectionActive = true;

        ConnectionSender.SendConnectionRequest();
    }

    public override void Disconnect(string reason = "")
    {
        // Make sure we are currently in a server
        if (!_isServerActive && !_isConnectionActive)
            return;

        try
        {
            SendToProxyServer(MessageTypes.Disconnect);
        }
        catch
        {
            FusionLogger.Log("Error closing socket server / connection manager");
        }

        _isServerActive = false;
        _isConnectionActive = false;

        InternalServerHelpers.OnDisconnect(reason);
    }

    public string ServerCode { get; private set; } = null;

    public override string GetServerCode()
    {
        return ServerCode;
    }

    public override void RefreshServerCode()
    {
        ServerCode = RandomCodeGenerator.GetString(8);

        LobbyInfoManager.PushLobbyUpdate();
    }

    public override void JoinServerByCode(string code)
    {
        if (Matchmaker == null)
        {
            return;
        }

#if DEBUG
        FusionLogger.Log($"Searching for servers with code {code}...");
#endif

        Matchmaker.RequestLobbies((info) =>
        {
            foreach (var lobby in info.Lobbies)
            {
                var lobbyCode = lobby.Metadata.LobbyInfo.LobbyCode;
                var inputCode = code;

#if DEBUG
                FusionLogger.Log($"Found server with code {lobbyCode}");
#endif

                // Case insensitive
                // Makes it easier to input
                if (lobbyCode.ToLower() == code.ToLower())
                {
                    JoinServer(lobby.Metadata.LobbyInfo.LobbyId);
                    break;
                }
            }
        });
    }

    private void HookSteamEvents()
    {
        // Add server hooks
        MultiplayerHooking.OnPlayerJoined += OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft += OnPlayerLeave;
        MultiplayerHooking.OnDisconnected += OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged += OnUpdateLobby;

        _currentLobby = new ProxyNetworkLobby();
    }

    private void OnPlayerJoin(PlayerID id)
    {
        if (!id.IsMe)
            _voiceManager.GetSpeaker(id);

        OnUpdateLobby();
    }

    private void OnPlayerLeave(PlayerID id)
    {
        _voiceManager.RemoveSpeaker(id);
    }

    private void OnDisconnect()
    {
        _voiceManager.ClearManager();
    }

    private void UnHookSteamEvents()
    {
        // Remove server hooks
        MultiplayerHooking.OnPlayerJoined -= OnPlayerJoin;
        MultiplayerHooking.OnPlayerLeft -= OnPlayerLeave;
        MultiplayerHooking.OnDisconnected -= OnDisconnect;

        LobbyInfoManager.OnLobbyInfoChanged -= OnUpdateLobby;
    }

    public void OnUpdateLobby()
    {
        // Make sure the lobby exists
        if (Lobby == null)
        {
#if DEBUG
            FusionLogger.Warn("Tried updating the proxy lobby, but it was null!");
#endif
            return;
        }

        // Write active info about the lobby
        LobbyMetadataSerializer.WriteInfo(Lobby);

        // Request Steam Friends
        NetDataWriter writer = NewWriter(MessageTypes.SteamFriends);
        SendToProxyServer(writer);
    }
}
