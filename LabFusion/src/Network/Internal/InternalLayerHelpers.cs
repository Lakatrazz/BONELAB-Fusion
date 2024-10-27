using LabFusion.Preferences.Client;
using LabFusion.Player;
using LabFusion.BoneMenu;

namespace LabFusion.Network
{
    /// <summary>
    /// Internal class used for creating network layers and updating them.
    /// </summary>
    public static class InternalLayerHelpers
    {
        public static NetworkLayer CurrentNetworkLayer { get; private set; }

        public static void SetLayer(NetworkLayer layer)
        {
            CurrentNetworkLayer = layer;
            CurrentNetworkLayer.OnInitializeLayer();
        }

        public static void UpdateLoadedLayer()
        {
            // Make sure the layer being loaded isn't already loaded
            var title = ClientSettings.NetworkLayerTitle.Value;
            if (!NetworkLayer.LayerLookup.TryGetValue(title, out var layer))
                return;
            layer = NetworkLayerDeterminer.VerifyLayer(layer);
            if (CurrentNetworkLayer == layer) 
                return;

            // Cleanup the network layer
            OnCleanupLayer();

            // We're just going to *assume* that the layer that is being loaded isn't null since the layer title loaded fine...
            NetworkLayerDeterminer.LoadLayer();

            SetLayer(NetworkLayerDeterminer.LoadedLayer);

            // Recreate Bonemenu
            BoneMenuCreator.OnPopulateMainPage();

            BoneMenuCreator.OpenMainPage();
        }

        public static void OnLateInitializeLayer()
        {
            CurrentNetworkLayer?.OnLateInitializeLayer();
        }

        public static void OnCleanupLayer()
        {
            CurrentNetworkLayer?.OnCleanupLayer();

            CurrentNetworkLayer = null;
        }

        public static void OnUpdateLayer()
        {
            CurrentNetworkLayer?.OnUpdateLayer();
        }

        public static void OnLateUpdateLayer()
        {
            CurrentNetworkLayer?.OnLateUpdateLayer();
        }

        public static void OnGUILayer()
        {
            CurrentNetworkLayer?.OnGUILayer();
        }

        public static void OnUserJoin(PlayerId id)
        {
            CurrentNetworkLayer?.OnUserJoin(id);
        }
    }
}
