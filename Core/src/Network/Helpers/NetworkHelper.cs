using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    /// <summary>
    /// Helper class for calling basic methods on the Server or Client.
    /// </summary>
    public static class NetworkHelper {
        /// <summary>
        /// Starts a server if there is currently none active.
        /// </summary>
        public static void StartServer() {
            if (NetworkInfo.CurrentNetworkLayer != null) {
                NetworkInfo.CurrentNetworkLayer.StartServer();
            }
        }

        /// <summary>
        /// Stops an existing server.
        /// </summary>
        public static void StopServer() {
            if (NetworkInfo.CurrentNetworkLayer != null && NetworkInfo.IsServer) {
                NetworkInfo.CurrentNetworkLayer.Disconnect();
            }
        }

        /// <summary>
        /// Disconnects the network layer and cleans up.
        /// </summary>
        public static void Disconnect() {
            if (NetworkInfo.CurrentNetworkLayer != null)
                NetworkInfo.CurrentNetworkLayer.Disconnect();
        }
    }
}
