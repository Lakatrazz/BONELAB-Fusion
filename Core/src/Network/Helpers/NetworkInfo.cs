using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public static class NetworkInfo {
        public static NetworkLayer CurrentNetworkLayer => InternalLayerHelpers.CurrentNetworkLayer;

        public static bool HasServer => CurrentNetworkLayer.IsServer || CurrentNetworkLayer.IsClient;
        public static bool IsServer => CurrentNetworkLayer.IsServer;
        public static bool IsClient => CurrentNetworkLayer.IsClient && !CurrentNetworkLayer.IsServer;
    }
}
