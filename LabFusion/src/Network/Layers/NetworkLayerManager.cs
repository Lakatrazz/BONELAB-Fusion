using LabFusion.Utilities;

using System.Reflection;

namespace LabFusion.Network;

/// <summary>
/// Manages the registration and initialization of NetworkLayers.
/// </summary>
public static class NetworkLayerManager
{
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
    /// The active network transport layer.
    /// </summary>
    public static NetworkLayer Layer { get; private set; } = null;

    /// <summary>
    /// Returns if there is an active network layer.
    /// </summary>
    public static bool HasLayer => Layer != null;

    /// <summary>
    /// Returns if the user is logged into the active network layer.
    /// </summary>
    public static bool LoggedIn
    {
        get => _loggedIn;
        private set
        {
            _loggedIn = value;

            LogInChanged?.Invoke(value);
        }
    }

    /// <summary>
    /// Invoked when the user logs in or out of the active network layer.
    /// </summary>
    public static event Action<bool> LogInChanged;

    private static bool _loggedIn = false;

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
    /// Attempts to log in to a NetworkLayer.
    /// </summary>
    /// <param name="layer"></param>
    public static void LogIn(NetworkLayer layer)
    {
        layer.LogIn();
    }

    /// <summary>
    /// Attempts to log out of the currently logged in NetworkLayer.
    /// </summary>
    public static void LogOut()
    {
        if (Layer == null)
        {
            return;
        }

        Layer.LogOut();
    }

    internal static void OnInitializeMelon()
    {
        NetworkLayer.LogInCompleted += OnLoggedIn;
        NetworkLayer.LogOutCompleted += OnLoggedOut;
    }

    private static void OnLoggedIn(NetworkLayer layer)
    {
        var previousLayer = Layer;

        if (previousLayer != null && previousLayer != layer)
        {
            Layer = null;
            previousLayer.LogOut();
        }

        Layer = layer;

        layer.OnInitializeLayer();

        LoggedIn = true;
    }

    private static void OnLoggedOut(NetworkLayer layer)
    {
        layer.OnDeinitializeLayer();

        if (Layer == layer)
        {
            Layer = null;
            LoggedIn = false;
        }
    }
}
