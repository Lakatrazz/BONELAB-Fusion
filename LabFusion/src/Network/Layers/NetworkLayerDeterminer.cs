using LabFusion.Network.Proxy;
using LabFusion.Preferences.Client;
using LabFusion.Utilities;

namespace LabFusion.Network;

public static class NetworkLayerDeterminer
{
    public static NetworkLayer LoadedLayer { get; private set; }
    public static string LoadedTitle { get; private set; }

    public static NetworkLayer GetDefaultLayer()
    {
        if (PlatformHelper.IsAndroid)
        {
            return NetworkLayerManager.GetLayer<ProxySteamVRNetworkLayer>();
        }

        return NetworkLayerManager.GetLayer<SteamVRNetworkLayer>();
    }

    public static NetworkLayer VerifyLayer(NetworkLayer layer)
    {
        if (layer.CheckSupported() && layer.CheckValidation())
        {
            return layer;
        }
        else if (layer.TryGetFallback(out var fallback))
        {
            return VerifyLayer(fallback);
        }
        else
        {
            return NetworkLayerManager.GetLayer<EmptyNetworkLayer>();
        }
    }

    public static void LoadLayer()
    {
        var title = ClientSettings.NetworkLayerTitle.Value;

        if (!NetworkLayerManager.LayerTitleLookup.TryGetValue(title, out var layer))
        {
            layer = GetDefaultLayer();
        }

        layer = VerifyLayer(layer);

        LoadedLayer = layer;
        LoadedTitle = layer.Title;
    }
}