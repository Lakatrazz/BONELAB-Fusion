using LabFusion.Data;
using LabFusion.Player;
using LabFusion.Utilities;
using LabFusion.Network;

using Steamworks;
using Steamworks.Data;

namespace MarrowFusion.Steam;

public abstract class SteamNetworkLayer : NetworkLayer
{
    public const int ReceiveBufferSize = 32;

    public abstract uint AppID { get; }

    public override string Title => "Steam";

    public override string Platform => "Steam";

    public override bool IsServerRunning => ServerSteamSocket != null;

    public override ServerID RunningServerID => _runningServerID;

    public override bool IsClientConnected => ClientSteamConnection != null;

    public override ClientPlatformID? ClientID => new(ClientSteamID);

    public override ServerID ConnectedServerID => _connectedServerID;

    public override NetworkLobby Lobby => _currentLobby;

    public override Matchmaker Matchmaker => _matchmaker;

    public override bool IsFriendsListSupported => true;

    /// <summary>
    /// The steam client's logged in SteamID.
    /// </summary>
    public static SteamId ClientSteamID { get; private set; }

    /// <summary>
    /// The server's steam socket manager, if a server is running.
    /// </summary>
    public static SteamSocketManager ServerSteamSocket { get; private set; } = null;

    /// <summary>
    /// The client's steam connection manager, if a client is connected to a server.
    /// </summary>
    public static SteamConnectionManager ClientSteamConnection { get; private set; } = null;

    // A local reference to a lobby
    // This isn't actually used for joining servers, just for matchmaking
    protected Lobby _localLobby;

    private ServerID _runningServerID = ServerID.Empty;
    private ServerID _connectedServerID = ServerID.Empty;

    private Matchmaker _matchmaker = null;
    private SteamLobby _currentLobby;

    public override bool CheckSupported() => !PlatformHelper.IsAndroid;

    public override bool CheckValidation() => SteamAPILoader.IsAPIAvailable();

    public override void SendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID client)
    {
        if (!IsServerRunning)
        {
            return;
        }

        ServerSteamSocket.SendToClient(client, channel, message);
    }

    public override void SendToClients(NetMessage message, NetworkChannel channel, ReadOnlySpan<ClientPlatformID> clients)
    {
        if (!IsServerRunning)
        {
            return;
        }

        ServerSteamSocket.SendToClients(clients, channel, message);
    }

    public override void SendToServer(NetMessage message, NetworkChannel channel)
    {
        if (!IsClientConnected)
        {
            return;
        }

        ClientSteamConnection.SendToServer(channel, message);
    }

    public override bool IsFriend(ClientPlatformID client)
    {
        return client == ClientID || new Friend((ulong)client).IsFriend;
    }

    protected override async Task<bool> TryLogInAsync(CancellationToken cancellationToken)
    {
        var loadAPITask = await ThreadHelper.RunOnMainThreadAsTask(SteamAPILoader.TryLoadSteamAPI);

        if (!loadAPITask)
        {
            return false;
        }

        if (SteamClient.IsValid)
        {
            return false;
        }

        await ThreadHelper.RunOnMainThreadAsTask(SteamGameClientManager.Shutdown);

        bool succeeded;

        try
        {
            SteamClient.Init(AppID, false);

            succeeded = true;
        }
        catch (Exception e)
        {
            SteamModule.Logger.LogException("initializing Steamworks", e);

            succeeded = false;
        }

        return succeeded;
    }

    protected override Task<bool> TryLogOutAsync(CancellationToken cancellationToken)
    {
        SteamClient.Shutdown();

        return Task.FromResult(true);
    }

    protected override Task<bool> TryStartServerAsync(CancellationToken cancellationToken)
    {
        ServerSteamSocket = SteamNetworkingSockets.CreateRelaySocket<SteamSocketManager>();
        _runningServerID = new ServerID(ClientSteamID);

        TryRefreshServerCode();

        return Task.FromResult(true);
    }

    protected override Task<bool> TryStopServerAsync(CancellationToken cancellationToken)
    {
        try
        {
            ServerSteamSocket?.Close();
        }
        catch (Exception e)
        {
            SteamModule.Logger.LogException("stopping server", e);

            return Task.FromResult(false);
        }

        ServerSteamSocket = null;
        _runningServerID = ServerID.Empty;

        return Task.FromResult(true);
    }

    protected override Task<bool> TryDisconnectClientAsync(ClientPlatformID client, CancellationToken cancellationToken)
    {
        ServerSteamSocket.DisconnectUser((ulong)client);

        return Task.FromResult(true);
    }

    protected override async Task<bool> TryConnectToServerAsync(ServerID server, CancellationToken cancellationToken)
    {
        SteamId serverSteamID = (ulong)server;

        var connection = SteamNetworkingSockets.ConnectRelay<SteamConnectionManager>(serverSteamID);

        while (connection.Connecting)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                connection.Close();
                return false;
            }

            await Task.Delay(50, CancellationToken.None);
        }

        if (!connection.Connected)
        {
            return false;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            connection.Close();
            return false;
        }

        ClientSteamConnection = connection;
        _connectedServerID = server;

        return true;
    }

    protected override async Task<bool> TryDisconnectFromServerAsync(CancellationToken cancellationToken)
    {
        if (ClientSteamConnection == null)
        {
            return false;
        }

        try
        {
            if (ClientSteamConnection.Connected)
            {
                ClientSteamConnection.Close();
            }
        }
        catch (Exception e)
        {
            SteamModule.Logger.LogException("disconnecting client from server", e);

            return false;
        }

        while (ClientSteamConnection != null && ClientSteamConnection.Connected)
        {
            await Task.Delay(50, CancellationToken.None);
        }

        ClientSteamConnection = null;
        _connectedServerID = ServerID.Empty;

        return true;
    }

    protected override Task<string> TryGetLocalUsernameAsync() => Task.FromResult(SteamClient.Name);

    protected override async Task<string> TryGetClientUsernameAsync(ClientPlatformID client)
    {
        var friend = new Friend((ulong)client);

        await friend.RequestInfoAsync();

        return friend.Name;
    }

    protected override void OnInitialize()
    {
        if (!SteamClient.IsValid)
        {
            SteamModule.Logger.Error("Steamworks failed to initialize!");
            return;
        }

        // Get steam information
        ClientSteamID = SteamClient.SteamId;

        SteamModule.Logger.Log($"Steamworks initialized with SteamID {ClientSteamID} and ApplicationID {AppID}!");

        SteamNetworkingUtils.InitRelayNetworkAccess();

        HookEvents();

        _matchmaker = new SteamMatchmaker();
    }

    protected override void OnDeinitialize()
    {
        _matchmaker = null;

        _localLobby = default;
        _currentLobby = null;

        UnhookEvents();

        SteamAPI.Shutdown();
    }

    protected override void OnTick()
    {
        // Run callbacks for our client
        SteamClient.RunCallbacks();

        // Receive any needed messages
        try
        {
            ServerSteamSocket?.Receive(ReceiveBufferSize);

            ClientSteamConnection?.Receive(ReceiveBufferSize);
        }
        catch (Exception e)
        {
            SteamModule.Logger.LogException("receiving data on Socket and Connection", e);
        }
    }

    private async void AwaitLobbyCreation()
    {
        var lobbyTask = await SteamMatchmaking.CreateLobbyAsync();
        
        if (!lobbyTask.HasValue)
        {
#if DEBUG
            SteamModule.Logger.Log("Failed to create a steam lobby!");
#endif
            return;
        }

        _localLobby = lobbyTask.Value;

        _currentLobby = new SteamLobby(_localLobby);
        _currentLobby.SetServerID(ClientSteamID);
    }

    private void HookEvents()
    {
        HookServer();
        HookRichPresence();

        // Create a local lobby
        AwaitLobbyCreation();
    }

    private void UnhookEvents()
    {
        UnhookServer();
        UnhookRichPresence();

        // Remove the local lobby
        if (_localLobby.Id == ClientSteamID)
        {
            _localLobby.Leave();
            _currentLobby = null;
        }
    }

    private void HookServer()
    {
        NetworkManager.ServerEstablished += OnServerEstablished;
        NetworkManager.ServerLost += OnServerLost;

        PlayerIDManager.PlayerJoined += OnPlayerJoined;
        PlayerIDManager.PlayerLeft += OnPlayerLeft;
    }

    private void UnhookServer()
    {
        NetworkManager.ServerEstablished -= OnServerEstablished;
        NetworkManager.ServerLost -= OnServerLost;

        PlayerIDManager.PlayerJoined -= OnPlayerJoined;
        PlayerIDManager.PlayerLeft -= OnPlayerLeft;
    }

    private void OnServerEstablished() => UpdateRichPresence();
    private void OnServerLost() => UpdateRichPresence();

    private void OnPlayerJoined(PlayerID playerID) => UpdateRichPresence();
    private void OnPlayerLeft(PlayerID playerID) => UpdateRichPresence();

    private void HookRichPresence()
    {
        SteamFriends.OnGameRichPresenceJoinRequested += OnGameRichPresenceJoinRequested;
    }

    private void UnhookRichPresence()
    {
        SteamFriends.OnGameRichPresenceJoinRequested -= OnGameRichPresenceJoinRequested;
    }

    private void OnGameRichPresenceJoinRequested(Friend friend, string connect)
    {
        var group = friend.GetRichPresence("steam_player_group");

        if (string.IsNullOrWhiteSpace(group))
        {
            return;
        }

        var server = new ServerID(group);

        ConnectToServer(server);
    }

    private void UpdateRichPresence()
    {
        if (!HasServer)
        {
            ClearRichPresence();
            return;
        }

        if (!SteamClient.IsValid)
        {
            return;
        }

        var playerCount = PlayerIDManager.PlayerCount;

        SteamFriends.SetRichPresence("connect", "true");
        SteamFriends.SetRichPresence("steam_player_group", ServerID.ToString());
        SteamFriends.SetRichPresence("steam_player_group_size", playerCount.ToString());
    }

    private static void ClearRichPresence() 
    { 
        if (!SteamClient.IsValid)
        {
            return;
        }

        SteamFriends.ClearRichPresence(); 
    }
}