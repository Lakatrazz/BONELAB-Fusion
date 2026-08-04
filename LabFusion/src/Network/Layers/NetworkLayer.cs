using LabFusion.Voice;

namespace LabFusion.Network;

/// <summary>
/// Privacy type for a server.
/// </summary>
public enum ServerPrivacy
{
    PUBLIC = 0,
    PRIVATE = 1,
    FRIENDS_ONLY = 2,
    LOCKED = 3,
}

/// <summary>
/// The foundational class for a server's networking system.
/// </summary>
public abstract class NetworkLayer
{
    /// <summary>
    /// Invoked when a NetworkLayer finishes logging in.
    /// </summary>
    public static event Action<NetworkLayer> LogInCompleted;

    /// <summary>
    /// Invoked when a logged in NetworkLayer finishes logging out.
    /// </summary>
    public static event Action<NetworkLayer> LogOutCompleted;

    /// <summary>
    /// Invoked when a server is started on this instance and clients are able to connect.
    /// </summary>
    public static event Action ServerStarted;

    /// <summary>
    /// Invoked when the server running on this instance is stopped.
    /// </summary>
    public static event Action ServerStopped;

    /// <summary>
    /// Invoked when the client establishes a connection to the server and is able to send messages to the server.
    /// </summary>
    public static event Action ConnectionEstablished;

    /// <summary>
    /// Invoked when the client has lost connection or was disconnected from the server.
    /// </summary>
    public static event Action ConnectionLost;

    private Type _type;
    private bool _hasType;

    /// <summary>
    /// The NetworkLayer's cached type.
    /// </summary>
    public Type Type
    {
        get
        {
            if (!_hasType)
            {
                _type = GetType();
                _hasType = true;
            }

            return _type;
        }
    }

    /// <summary>
    /// The Title of this NetworkLayer to be displayed.
    /// </summary>
    public virtual string Title => Type.AssemblyQualifiedName;

    /// <summary>
    /// The Platform of this NetworkLayer. Necessary for validating platform ID related things such as bans.
    /// </summary>
    public abstract string Platform { get; }

    /// <summary>
    /// Returns true if a server is currently running through this NetworkLayer.
    /// <para>This will not run true if the NetworkLayer is only a client connected to the server and not hosting it.</para>
    /// </summary>
    public abstract bool IsServerRunning { get; }

    /// <summary>
    /// If a server is running, this will return the ID that the server is running on.
    /// Otherwise, it will return <see cref="ServerID.Empty"/>.
    /// </summary>
    public abstract ServerID RunningServerID { get; }

    /// <summary>
    /// Returns true if this NetworkLayer is running a client connected to a server.
    /// <para>This should still return true even if the server hasn't accepted the client's connection yet, as long as the client can send data to the server.</para>
    /// </summary>
    public abstract bool IsClientConnected { get; }

    /// <summary>
    /// Returns true if this NetworkLayer is running both a server and a client connected to that server.
    /// </summary>
    public virtual bool IsClientHost => IsClientConnected && IsServerRunning;

    /// <summary>
    /// If the client is connected to a server, this will return the ID of the server that the client is connected to.
    /// Otherwise, it will return <see cref="ServerID.Empty"/>.
    /// </summary>
    public abstract ServerID ConnectedServerID { get; }

    /// <summary>
    /// Returns the active lobby.
    /// </summary>
    public virtual INetworkLobby Lobby => null;

    /// <summary>
    /// Returns the used voice manager.
    /// </summary>
    public virtual IVoiceManager VoiceManager => null;

    /// <summary>
    /// Returns the layer's matchmaker for finding lobbies.
    /// </summary>
    public virtual IMatchmaker Matchmaker => null;

    /// <summary>
    /// Returns true if this NetworkLayer is supported on the current platform.
    /// </summary>
    /// <returns></returns>
    public abstract bool CheckSupported();

    /// <summary>
    /// Returns true if this NetworkLayer is valid and able to be ran.
    /// </summary>
    /// <returns></returns>
    public abstract bool CheckValidation();

    /// <summary>
    /// Returns a fallback layer if it exists in the event this layer fails.
    /// </summary>
    /// <param name="fallback"></param>
    /// <returns></returns>
    public virtual bool TryGetFallback(out NetworkLayer fallback)
    {
        fallback = null;
        return false;
    }

    /// <summary>
    /// Start running a server. 
    /// This will not automatically connect to the server as a client.
    /// <para>When implementing, a successful server start should invoke <see cref="InvokeServerStartedEvent"/> afterwards so that the proper callbacks are received.</para>
    /// </summary>
    public abstract void StartServer();

    /// <summary>
    /// If a server is currently running, stop the server.
    /// <para>When implementing, a successful stop of the server should invoke <see cref="InvokeServerStoppedEvent"/> afterwards so that the proper callbacks are received.</para>
    /// </summary>
    public abstract void StopServer();

    /// <summary>
    /// If a server is currently running, disconnect a client from the server.
    /// </summary>
    /// <param name="client"></param>
    public abstract void ServerDisconnectClient(ClientPlatformID client);

    /// <summary>
    /// Connect the client to a server.
    /// <para>When implementing, a successful connection should invoke <see cref="InvokeConnectionEstablishedEvent"/> afterwards so that the proper callbacks are received.</para>
    /// </summary>
    /// <param name="server"></param>
    public abstract void ConnectToServer(ServerID server);

    /// <summary>
    /// If the client is currently connected to a server, disconnect from the server.
    /// <para>When implementing, a successful disconnect should invoke <see cref="InvokeConnectionLostEvent"/> afterwards so that the proper callbacks are received.</para>
    /// </summary>
    public abstract void ClientDisconnectFromServer();

    /// <summary>
    /// Disconnects the client from the connection and/or server.
    /// </summary>
    public abstract void Disconnect(string reason = "");

    /// <summary>
    /// Forcefully closes the connection for a connected user.
    /// </summary>
    /// <param name="platformID">The PlatformID of the connected user.</param>
    public abstract void DisconnectUser(ClientPlatformID platformID);

    /// <summary>
    /// Returns the username of the player with id userId.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public virtual string GetUsername(ClientPlatformID platformID) => "Unknown";

    /// <summary>
    /// Returns true if this is a friend (ex. steam friends).
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public virtual bool IsFriend(ClientPlatformID platformID) => false;

    /// <summary>
    /// If a server is running, send a message from the server to a specific client.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="clientPlatformID"></param>
    public abstract void ServerSendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID);

    /// <summary>
    /// If a server is running, send a message from the server to multiple clients.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    /// <param name="clientPlatformIDs"></param>
    public abstract void ServerSendToClients(NetMessage message, NetworkChannel channel, Span<ClientPlatformID> clientPlatformIDs);

    /// <summary>
    /// If a client is connected to a server, send a message from the client to the server.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    public abstract void ClientSendToServer(NetMessage message, NetworkChannel channel);

    /// <summary>
    /// Invoked on the layer after it has logged in to any necessary APIs.
    /// </summary>
    public abstract void OnInitializeLayer();

    /// <summary>
    /// Invoked on the layer after it has logged out of any necessary APIs.
    /// <para>This is when you should clean up the layer.</para>
    /// </summary>
    public abstract void OnDeinitializeLayer();

    /// <summary>
    /// Attempts to log in to the NetworkLayer.
    /// <para>When implementing, a successful login should invoke <see cref="InvokeLoggedInEvent"/> afterwards so that the proper callbacks are received.</para>
    /// </summary>
    public abstract void LogIn();

    /// <summary>
    /// Attempts to log out of the NetworkLayer.
    /// <para>When implementing, a successful log out should invoke <see cref="InvokeLoggedOutEvent"/> afterwards so that the proper callbacks are received.</para>
    /// </summary>
    public abstract void LogOut();

    public virtual void OnUpdateLayer() { }

    public virtual void OnLateUpdateLayer() { }

    public virtual string GetServerCode()
    {
        return null;
    }

    public virtual void RefreshServerCode()
    {
    }

    public virtual void JoinServerByCode(string code)
    {
        throw new NotImplementedException("The current NetworkLayer does not support joining by code!");
    }

    protected void InvokeLoggedInEvent() => LogInCompleted?.Invoke(this);
    protected void InvokeLoggedOutEvent() => LogOutCompleted?.Invoke(this);

    protected void InvokeServerStartedEvent() => ServerStarted?.Invoke();
    protected void InvokeServerStoppedEvent() => ServerStopped?.Invoke();

    protected void InvokeConnectionEstablishedEvent() => ConnectionEstablished?.Invoke();
    protected void InvokeConnectionLostEvent() => ConnectionLost?.Invoke();
}