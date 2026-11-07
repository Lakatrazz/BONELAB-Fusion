using LabFusion.Extensions;
using LabFusion.Player;
using LabFusion.UI.Popups;
using LabFusion.Utilities;

using System.Reflection;

namespace LabFusion.Network;

public delegate void NetworkLayerDelegate(NetworkLayer layer);

/// <summary>
/// Manages the registration and initialization of NetworkLayers.
/// </summary>
public static class NetworkLayerManager
{
    /// <summary>
    /// The default NetworkLayer platform if there is no network layer.
    /// </summary>
    public static readonly string DefaultPlatform = "None";

    /// <summary>
    /// The list of loaded NetworkLayers.
    /// </summary>
    public static readonly List<NetworkLayer> Layers = new();

    /// <summary>
    /// The list of loaded NetworkLayers that are supported by the current platform.
    /// </summary>
    public static readonly List<NetworkLayer> SupportedLayers = new();

    /// <summary>
    /// A lookup table for a NetworkLayer based on its title.
    /// </summary>
    public static readonly Dictionary<string, NetworkLayer> LayerTitleLookup = new();

    /// <summary>
    /// The active network layer.
    /// </summary>
    public static NetworkLayer Layer { get; private set; } = null;

    /// <summary>
    /// Returns if there is an active network layer.
    /// </summary>
    public static bool HasLayer => Layer != null;

    /// <summary>
    /// The platform based on the active NetworkLayer, or <see cref="DefaultPlatform"/> if there is no NetworkLayer active.
    /// </summary>
    public static string Platform => Layer?.Platform ?? DefaultPlatform;

    /// <summary>
    /// Returns if the active layer is currently being logged in to.
    /// </summary>
    public static bool IsLoggingIn { get; private set; }

    /// <summary>
    /// Returns if the active layer is currently being logged out of.
    /// </summary>
    public static bool IsLoggingOut { get; private set; }

    /// <summary>
    /// Returns if the active layer is already logged in.
    /// </summary>
    public static bool IsLoggedIn { get; private set; }

    /// <summary>
    /// Invoked whenever the user begins logging into a NetworkLayer.
    /// </summary>
    public static event NetworkLayerDelegate LogInStarted;

    /// <summary>
    /// Invoked whenever the user has successfully logged into a NetworkLayer.
    /// </summary>
    public static event NetworkLayerDelegate LogInCompleted;

    /// <summary>
    /// Invoked whenever the user attempted to log into a NetworkLayer, but failed.
    /// </summary>
    public static event NetworkLayerDelegate LogInFailed;

    /// <summary>
    /// Invokes whenever the user begins logging out of a NetworkLayer.
    /// </summary>
    public static event NetworkLayerDelegate LogOutStarted;

    /// <summary>
    /// Invoked whenever the user has successfully logged out of the NetworkLayer.
    /// </summary>
    public static event NetworkLayerDelegate LogOutCompleted;

    /// <summary>
    /// Invoked whenever the user attempted to log out of the NetworkLayer, but failed.
    /// </summary>
    public static event NetworkLayerDelegate LogOutFailed;

    /// <summary>
    /// Registers all <see cref="NetworkLayer"/>s contained in an assembly.
    /// </summary>
    /// <param name="assembly"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void LoadLayers(Assembly assembly)
    {
        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        AssemblyUtilities.LoadAllValid<NetworkLayer>(assembly, RegisterLayer);
    }

    /// <summary>
    /// Registers a <see cref="NetworkLayer"/> from a type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static void RegisterLayer<T>() where T : NetworkLayer => RegisterLayer(typeof(T));

    /// <summary>
    /// Registers a <see cref="NetworkLayer"/> from a type.
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="Exception"></exception>
    public static void RegisterLayer(Type type)
    {
        NetworkLayer layer = Activator.CreateInstance(type) as NetworkLayer;

        if (string.IsNullOrWhiteSpace(layer.Title))
        {
            FusionLogger.Warn($"Didn't register {type.Name} because its Title was invalid!");
            return;
        }

        if (LayerTitleLookup.ContainsKey(layer.Title))
        {
            throw new Exception($"{type.Name} has the same Title as {LayerTitleLookup[layer.Title].GetType().Name}, we can't replace layers!");
        }

        Layers.Add(layer);
        LayerTitleLookup.Add(layer.Title, layer);

        if (layer.CheckSupported())
        {
            SupportedLayers.Add(layer);
        }
    }

    /// <summary>
    /// Attempts to get a <see cref="NetworkLayer"/> instance from its type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static bool TryGetLayer<T>(out T layer) where T : NetworkLayer
    {
        layer = GetLayer<T>();
        return layer != null;
    }

    /// <summary>
    /// Gets a <see cref="NetworkLayer"/> instance from its type or returns null if it has not been registered.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetLayer<T>() where T : NetworkLayer
    {
        return (T)Layers.Find((l) => l.Type == typeof(T));
    }

    /// <summary>
    /// Gets the set target NetworkLayer. This is not the currently active network layer.
    /// For the active network layer, see <see cref="Layer"/>.
    /// </summary>
    /// <returns></returns>
    public static NetworkLayer GetTargetLayer()
    {
        NetworkLayerDeterminer.LoadLayer();

        return NetworkLayerDeterminer.LoadedLayer;
    }

    /// <summary>
    /// Logs the user into a specified NetworkLayer.
    /// <para>If the user is already logged in, they will be logged out of the existing layer before being logged into the new layer.</para>
    /// </summary>
    /// <param name="layer"></param>
    public static void LogIn(NetworkLayer layer)
    {
        Task.Run(async () => { await LogInAsync(layer); });
    }

    /// <summary>
    /// Logs the user into a specified NetworkLayer asynchronously.
    /// <para>If the user is already logged in, they will be logged out of the existing layer before being logged into the new layer.</para>
    /// </summary>
    /// <param name="layer"></param>
    /// <returns></returns>
    public static async Task<bool> LogInAsync(NetworkLayer layer) => await LogInAsync(layer, CancellationToken.None);

    /// <summary>
    /// Logs the user into a specified NetworkLayer asynchronously.
    /// <para>If the user is already logged in, they will be logged out of the existing layer before being logged into the new layer.</para>
    /// </summary>
    /// <param name="layer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> LogInAsync(NetworkLayer layer, CancellationToken cancellationToken)
    {
        if (IsLoggingIn || IsLoggingOut)
        {
            return false;
        }

        if (IsLoggedIn)
        {
            bool loggedOut = await LogOutAsync(cancellationToken);

            if (!loggedOut)
            {
                return false;
            }
        }

        IsLoggingIn = true;
        Layer = layer;

        ThreadHelper.RunOnMainThread(() => { OnLogInStarted(layer); });

        bool result = false;

        try
        {
            result = await layer.LogInAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result = false;

            FusionLogger.LogException("logging into NetworkLayer", ex);
        }
        finally
        {
            IsLoggingIn = false;
        }

        if (result)
        {
            ThreadHelper.RunOnMainThread(() => { OnLogInCompleted(layer); });
        }
        else
        {
            ThreadHelper.RunOnMainThread(() => { OnLogInFailed(layer); });
        }

        return result;
    }

    /// <summary>
    /// Logs the user out of the active NetworkLayer.
    /// </summary>
    public static void LogOut() => Task.Run(async () => { await LogOutAsync(); });

    /// <summary>
    /// Logs the user out of the active NetworkLayer asynchronously.
    /// </summary>
    /// <returns></returns>
    public static async Task<bool> LogOutAsync() => await LogOutAsync(CancellationToken.None);

    /// <summary>
    /// Logs the user out of the active NetworkLayer asynchronously.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<bool> LogOutAsync(CancellationToken cancellationToken)
    {
        if (IsLoggingIn || IsLoggingOut)
        {
            return false;
        }

        if (!IsLoggedIn)
        {
            return false;
        }

        IsLoggingOut = true;

        var layer = Layer;

        ThreadHelper.RunOnMainThread(() => { OnLogOutStarted(layer); });

        bool result = false;

        try
        {
            result = await layer.LogOutAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            result = false;

            FusionLogger.LogException("logging out of NetworkLayer", ex);
        }
        finally
        {
            IsLoggingOut = false;
        }

        if (result)
        {
            ThreadHelper.RunOnMainThread(() => { OnLogOutCompleted(layer); });
        }
        else
        {
            ThreadHelper.RunOnMainThread(() => { OnLogOutFailed(layer); });
        }

        return result;
    }

    internal static void Tick()
    {
        if (!HasLayer)
        {
            return;
        }

        try
        {
            Layer.Tick();
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("ticking network layer", ex);
        }
    }

    internal static void LateTick()
    {
        if (!HasLayer)
        {
            return;
        }

        try
        {
            Layer.LateTick();
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("late ticking network layer", ex);
        }
    }

    internal static void Initialize()
    {
        NetworkLayer.ServerStarted += OnServerStarted;
        NetworkLayer.ServerStopped += OnServerStopped;
        NetworkLayer.ServerCodeChanged += OnServerCodeChanged;

        NetworkLayer.ClientDisconnected += OnClientDisconnected;

        NetworkLayer.ConnectionEstablished += OnConnectionEstablished;
        NetworkLayer.ConnectionLost += OnConnectionLost;
    }

    private static void OnServerStarted()
    {
        ServerManager.OnServerStarted();
    }

    private static void OnServerStopped()
    {
        ServerManager.OnServerStopped();
    }

    private static void OnServerCodeChanged(string code) => LobbyInfoManager.PushLobbyUpdate();

    private static void OnClientDisconnected(ClientPlatformID client)
    {
        ServerManager.OnClientDisconnected(client);
    }

    private static void OnConnectionEstablished()
    {
        var clientID = Layer.ClientID;
        PlayerIDManager.SetLocalPlatformID(clientID);

        if (clientID == null)
        {
            FusionLogger.Error("A connection was established, but the NetworkLayer does not have a ClientID ready! Make sure that the ClientID is set before completing a connection!");

            Layer.DisconnectFromServer();
            return;
        }

        ClientManager.OnConnectionEstablished();
    }

    private static void OnConnectionLost()
    {
        ClientManager.OnConnectionLost();
    }

    private static void OnLogInStarted(NetworkLayer layer)
    {
        LogInStarted?.InvokeSafe(layer, "invoking LogInStarted event");
    }

    private static void OnLogInCompleted(NetworkLayer layer)
    {
        try
        {
            layer.Initialize();
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("initializing NetworkLayer", ex);
        }

        Layer = layer;
        IsLoggedIn = true;

        PlayerIDManager.SetLocalPlatformID(layer.ClientID);

        LogInCompleted?.InvokeSafe(layer, "invoking LogInCompleted event");
    }

    private static void OnLogInFailed(NetworkLayer layer)
    {
        Notifier.Send(new Notification()
        {
            Title = "Log In Failed",
            Message = $"Failed logging into {layer.Title}!",
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.ERROR,
            PopupLength = 6f,
        });

        Layer = null;
        IsLoggedIn = false;

        LogInFailed?.InvokeSafe(layer, "invoking LogInFailed event");
    }

    private static void OnLogOutStarted(NetworkLayer layer)
    {
        LogOutStarted?.InvokeSafe(layer, "invoking LogOutStarted event");
    }

    private static void OnLogOutCompleted(NetworkLayer layer)
    {
        try
        {
            layer.Deinitialize();
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("deinitializing NetworkLayer", ex);
        }

        Layer = null;
        IsLoggedIn = false;

        PlayerIDManager.SetLocalPlatformID(null);

        LogOutCompleted?.InvokeSafe(layer, "invoking LogOutCompleted event");
    }

    private static void OnLogOutFailed(NetworkLayer layer)
    {
        Notifier.Send(new Notification()
        {
            Title = "Log Out Failed",
            Message = $"Failed logging out of {layer.Title}!",
            SaveToMenu = false,
            ShowPopup = true,
            Type = NotificationType.ERROR,
            PopupLength = 6f,
        });

        LogOutFailed?.InvokeSafe(layer, "invoking LogOutFailed event");
    }
}
