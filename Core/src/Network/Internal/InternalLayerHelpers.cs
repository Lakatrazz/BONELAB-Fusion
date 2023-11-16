using BoneLib.BoneMenu.Elements;

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

        internal static void OnVoiceChatUpdate()
        {
            CurrentNetworkLayer?.OnVoiceChatUpdate();
        }

        internal static void OnVoiceBytesReceived(PlayerId id, byte[] bytes)
        {
            CurrentNetworkLayer?.OnVoiceBytesReceived(id, bytes);
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
