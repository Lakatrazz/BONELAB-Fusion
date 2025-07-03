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
    public static event Action<NetworkLayer> OnLoggedInEvent, OnLoggedOutEvent;

    private Type _type;
    private bool _hasType;

    /// <summary>
    /// The Type of this NetworkLayer.
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
    /// Returns true if this layer is hosting a server.
    /// </summary>
    public virtual bool IsHost => false;

    /// <summary>
    /// Returns true if this layer is a client inside of a server. This also returns true for the host.
    /// </summary>
    public virtual bool IsClient => false;

    /// <summary>
    /// Returns true if the networking solution allows the server to send messages to the host (Actual Server Logic vs P2P).
    /// </summary>
    public virtual bool ServerCanSendToHost => true;

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
    /// Returns if this NetworkLayer requires valid player IDs. 
    /// Set this to true if the layer sets <see cref="NetworkInfo.LastReceivedUser"/> upon receiving messages.
    /// Defaults to false.
    /// </summary>
    public virtual bool RequiresValidId => false;

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
    /// Starts the server.
    /// </summary>
    public abstract void StartServer();

    /// <summary>
    /// Disconnects the client from the connection and/or server.
    /// </summary>
    public abstract void Disconnect(string reason = "");

    /// <summary>
    /// Returns the username of the player with id userId.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public virtual string GetUsername(ulong userId) => "Unknown";

    /// <summary>
    /// Returns true if this is a friend (ex. steam friends).
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public virtual bool IsFriend(ulong userId) => false;

    /// <summary>
    /// Sends the message to the specified user if this is a server.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public virtual void SendFromServer(byte userId, NetworkChannel channel, NetMessage message) { }

    /// <summary>
    /// Sends the message to the specified user if this is a server.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public virtual void SendFromServer(ulong userId, NetworkChannel channel, NetMessage message) { }

    /// <summary>
    /// Sends the message to the dedicated server.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public virtual void SendToServer(NetworkChannel channel, NetMessage message) { }

    /// <summary>
    /// Sends the message to the server if this is a client. Sends to all clients if this is a server.
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public virtual void BroadcastMessage(NetworkChannel channel, NetMessage message) { }

    /// <summary>
    /// If this is a server, sends this message back to all users except for the provided id.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public virtual void BroadcastMessageExcept(byte userId, NetworkChannel channel, NetMessage message, bool ignoreHost = true)
    {
        foreach (var id in PlayerIDManager.PlayerIDs)
        {
            if (id.SmallID != userId && (id.SmallID != 0 || !ignoreHost))
            {
                SendFromServer(id.SmallID, channel, message);
            }
        }
    }

    /// <summary>
    /// If this is a server, sends this message back to all users except for the provided id.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="channel"></param>
    /// <param name="message"></param>
    public virtual void BroadcastMessageExcept(ulong userId, NetworkChannel channel, NetMessage message, bool ignoreHost = true)
    {
        foreach (var id in PlayerIDManager.PlayerIDs)
        {
            if (id.PlatformID != userId && (id.SmallID != 0 || !ignoreHost))
            {
                SendFromServer(id.SmallID, channel, message);
            }
        }
    }

    public abstract void OnInitializeLayer();

    public abstract void OnDeinitializeLayer();

    public abstract void LogIn();

    public abstract void LogOut();

    protected void InvokeLoggedInEvent()
    {
        OnLoggedInEvent?.Invoke(this);
    }

    protected void InvokeLoggedOutEvent()
    {
        OnLoggedOutEvent?.Invoke(this);
    }

    public virtual void OnUpdateLayer() { }

    public virtual void OnLateUpdateLayer() { }

    public virtual void OnUserJoin(PlayerID id) { }

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
                SupportedLayers.Add(layer);
        }
    }

    public static bool TryGetLayer<T>(out T layer) where T : NetworkLayer
    {
        layer = (T)Layers.Find((l) => l.Type == typeof(T));
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