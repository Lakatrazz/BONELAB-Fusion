using LabFusion.Network.EpicGames;
using LabFusion.Preferences.Client;
using LabFusion.Utilities;

namespace LabFusion.Network;

public static class NetworkLayerDeterminer
{
    public static NetworkLayer LoadedLayer { get; private set; }
    public static string LoadedTitle { get; private set; }

    public static NetworkLayer GetDefaultLayer()
    {
        return NetworkLayer.GetLayer<EpicGamesNetworkLayer>();
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
            return NetworkLayer.GetLayer<EmptyNetworkLayer>();
        }
    }

    public static void LoadLayer()
    {
        var title = ClientSettings.NetworkLayerTitle.Value;

        if (!NetworkLayer.LayerLookup.TryGetValue(title, out var layer))
        {
            layer = GetDefaultLayer();
        }

        layer = VerifyLayer(layer);

        LoadedLayer = layer;
        LoadedTitle = layer.Title;
    }
}