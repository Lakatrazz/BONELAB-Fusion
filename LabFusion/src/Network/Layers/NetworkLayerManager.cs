namespace LabFusion.Network;

public static class NetworkLayerManager
{
    public static NetworkLayer Layer { get; private set; } = null;

    public static bool HasLayer => Layer != null;

    private static bool _loggedIn = false;
    public static bool LoggedIn
    {
        get
        {
            return _loggedIn;
        }
        private set
        {
            _loggedIn = value;

            OnLoggedInChanged?.Invoke(value);
        }
    }

    public static event Action<bool> OnLoggedInChanged;

    public static void OnInitializeMelon()
    {
        NetworkLayer.OnLoggedInEvent += OnLoggedIn;
        NetworkLayer.OnLoggedOutEvent += OnLoggedOut;
    }

    public static NetworkLayer GetTargetLayer()
    {
        NetworkLayerDeterminer.LoadLayer();

        return NetworkLayerDeterminer.LoadedLayer;
    }

    public static void LogIn(NetworkLayer layer)
    {
        layer.LogIn();
    }

    public static void LogOut()
    {
        if (Layer == null)
        {
            return;
        }

        Layer.LogOut();
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
