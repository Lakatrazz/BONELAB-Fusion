using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    public static class NetworkInfo {
        /// <summary>
        /// The current network interface. Not recommended to touch!
        /// </summary>
        public static NetworkLayer CurrentNetworkLayer => InternalLayerHelpers.CurrentNetworkLayer;

        /// <summary>
        /// Returns true if the user is currently in a server.
        /// </summary>
        public static bool HasServer => CurrentNetworkLayer.IsServer || CurrentNetworkLayer.IsClient;

        /// <summary>
        /// Returns true if this user is the host or server.
        /// </summary>
        public static bool IsServer => CurrentNetworkLayer.IsServer;

        /// <summary>
        /// Returns true if this user is a client and not the server or host.
        /// </summary>
        public static bool IsClient => CurrentNetworkLayer.IsClient && !CurrentNetworkLayer.IsServer;

        /// <summary>
        /// Returns true if the networking solution allows the server to send messages to the host (Actual Server Logic vs P2P).
        /// </summary>
        public static bool ServerCanSendToHost => CurrentNetworkLayer.ServerCanSendToHost;

        /// <summary>
        /// The amount of bytes downloaded this frame.
        /// </summary>
        public static int BytesDown { get; internal set; }

        /// <summary>
        /// The amount of bytes sent this frame.
        /// </summary>
        public static int BytesUp { get; internal set; }
    }
}
