using System.Reflection;

using LabFusion.Player;
using LabFusion.Utilities;
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
    public static event Action<NetworkLayer> LogInCompleted, LogOutCompleted;

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
    /// </summary>
    public abstract void StartServer();

    /// <summary>
    /// If a server is currently running, stop the server.
    /// </summary>
    public abstract void StopServer();

    /// <summary>
    /// If a server is currently running, disconnect a client from the server.
    /// </summary>
    /// <param name="client"></param>
    public abstract void ServerDisconnectClient(ClientPlatformID client);

    /// <summary>
    /// Connect the client to a server.
    /// </summary>
    /// <param name="server"></param>
    public abstract void ConnectToServer(ServerID server);

    /// <summary>
    /// If the client is currently connected to a server, disconnect from the server.
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

    protected void InvokeLoggedInEvent()
    {
        LogInCompleted?.Invoke(this);
    }

    protected void InvokeLoggedOutEvent()
    {
        LogOutCompleted?.Invoke(this);
    }

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

    public static void RegisterLayersFromAssembly(Assembly targetAssembly)
    {
        if (targetAssembly == null) throw new NullReferenceException("Can't register from a null assembly!");

#if DEBUG
        FusionLogger.Log($"Populating NetworkLayer list from {targetAssembly.GetName().Name}!");
#endif

        AssemblyUtilities.LoadAllValid<NetworkLayer>(targetAssembly, RegisterLayer);
    }

    public static void RegisterLayer<T>() where T : NetworkLayer => RegisterLayer(typeof(T));

    private static void RegisterLayer(Type type)
    {
        NetworkLayer layer = Activator.CreateInstance(type) as NetworkLayer;

        if (string.IsNullOrWhiteSpace(layer.Title))
        {
            FusionLogger.Warn($"Didn't register {type.Name} because its Title was invalid!");
        }
        else
        {
            if (LayerLookup.ContainsKey(layer.Title)) throw new Exception($"{type.Name} has the same Title as {LayerLookup[layer.Title].GetType().Name}, we can't replace layers!");

#if DEBUG
            FusionLogger.Log($"Registered {type.Name}");
#endif

            Layers.Add(layer);
            LayerLookup.Add(layer.Title, layer);

            if (layer.CheckSupported())
            {
                SupportedLayers.Add(layer);
            }
        }
    }

    public static bool TryGetLayer<T>(out T layer) where T : NetworkLayer
    {
        layer = GetLayer<T>();
        return layer != null;
    }

    public static T GetLayer<T>() where T : NetworkLayer
    {
        return (T)Layers.Find((l) => l.Type == typeof(T));
    }

    public static readonly List<NetworkLayer> Layers = new();
    public static readonly Dictionary<string, NetworkLayer> LayerLookup = new();
    public static readonly List<NetworkLayer> SupportedLayers = new();
}