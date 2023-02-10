using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Senders;

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
        public static void Disconnect(string reason = "") {
            if (NetworkInfo.CurrentNetworkLayer != null)
                NetworkInfo.CurrentNetworkLayer.Disconnect(reason);
        }

        /// <summary>
        /// Returns true if this user is friended on the active network platform.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static bool IsFriend(ulong userId) {
            if (NetworkInfo.CurrentNetworkLayer != null)
                return NetworkInfo.CurrentNetworkLayer.IsFriend(userId);

            return false;
        }

        /// <summary>
        /// Kicks a user from the game.
        /// </summary>
        /// <param name="id"></param>
        public static void KickUser(PlayerId id) {
            ConnectionSender.SendDisconnect(id, "Kicked from Server");
        }

        /// <summary>
        /// Bans a user from the game.
        /// </summary>
        /// <param name="id"></param>
        public static void BanUser(PlayerId id) {
            BanList.Ban(id.LongId, id.GetMetadata(MetadataHelper.UsernameKey), "Banned");
            ConnectionSender.SendDisconnect(id, "Banned from Server");
        }

        /// <summary>
        /// Checks if a user is banned.
        /// </summary>
        /// <param name="longId"></param>
        /// <returns></returns>
        public static bool IsBanned(ulong longId) {
            // Check the ban list
            foreach (var tuple in BanList.BannedUsers) {
                if (tuple.Item1 == longId)
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Pardons a user from the ban list.
        /// </summary>
        /// <param name="longId"></param>
        public static void PardonUser(ulong longId) {
            BanList.Pardon(longId);
        }
    }
}
