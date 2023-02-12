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

        internal static void SetLayer(NetworkLayer layer) {
            CurrentNetworkLayer = layer;
            CurrentNetworkLayer.OnInitializeLayer();
        }

        internal static void OnLateInitializeLayer() {
            if (CurrentNetworkLayer != null)
                CurrentNetworkLayer.OnLateInitializeLayer();
        }

        internal static void OnCleanupLayer() {
            if (CurrentNetworkLayer != null)
                CurrentNetworkLayer.OnCleanupLayer();

            CurrentNetworkLayer = null;
        }

        internal static void OnUpdateLayer() {
            if (CurrentNetworkLayer != null)
                CurrentNetworkLayer.OnUpdateLayer();
        }

        internal static void OnLateUpdateLayer() {
            if (CurrentNetworkLayer != null)
                CurrentNetworkLayer.OnLateUpdateLayer();
        }

        internal static void OnGUILayer() {
            if (CurrentNetworkLayer != null)
                CurrentNetworkLayer.OnGUILayer();
        }

        internal static void OnVoiceChatUpdate()
        {
            if (CurrentNetworkLayer != null)
                CurrentNetworkLayer.OnVoiceChatUpdate();
        }

        internal static void OnVoiceBytesReceived(PlayerId id, byte[] bytes)
        {
            if (CurrentNetworkLayer != null)
                CurrentNetworkLayer.OnVoiceBytesReceived(id, bytes);
        }

        internal static void OnSetupBoneMenuLayer(MenuCategory category) {
            if (CurrentNetworkLayer != null)
                CurrentNetworkLayer.OnSetupBoneMenu(category);
        }

        internal static void OnUserJoin(PlayerId id) {
            if (CurrentNetworkLayer != null)
                CurrentNetworkLayer.OnUserJoin(id);
        }
    }
}
