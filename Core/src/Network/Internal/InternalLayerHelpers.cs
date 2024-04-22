using BoneLib.BoneMenu.Elements;
using LabFusion.BoneMenu;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    /// <summary>
    /// Internal class used for creating network layers and updating them.
    /// </summary>
    internal static class InternalLayerHelpers
    {
        internal static NetworkLayer CurrentNetworkLayer { get; private set; }

        internal static void SetLayer(NetworkLayer layer)
        {
            CurrentNetworkLayer = layer;
            CurrentNetworkLayer.OnInitializeLayer();
        }

        internal static void UpdateLoadedLayer()
        {
            // Make sure the layer being loaded isn't already loaded
            var title = FusionPreferences.ClientSettings.NetworkLayerTitle.GetValue();
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
            BoneLib.BoneMenu.MenuManager.SelectCategory(FusionPreferences.fusionCategory);
            FusionPreferences.OnCreateBoneMenu();
        }

        internal static void OnLateInitializeLayer()
        {
            CurrentNetworkLayer?.OnLateInitializeLayer();
        }

        internal static void OnCleanupLayer()
        {
            CurrentNetworkLayer?.OnCleanupLayer();

            CurrentNetworkLayer = null;
        }

        internal static void OnUpdateLayer()
        {
            CurrentNetworkLayer?.OnUpdateLayer();
        }

        internal static void OnLateUpdateLayer()
        {
            CurrentNetworkLayer?.OnLateUpdateLayer();
        }

        internal static void OnGUILayer()
        {
            CurrentNetworkLayer?.OnGUILayer();
        }

        internal static void OnUpdateLobby()
        {
            CurrentNetworkLayer?.OnUpdateLobby();
        }

        internal static void OnSetupBoneMenuLayer(MenuCategory category)
        {
            CurrentNetworkLayer?.OnSetupBoneMenu(category);
        }

        internal static void OnUserJoin(PlayerId id)
        {
            CurrentNetworkLayer?.OnUserJoin(id);
        }
    }
}
