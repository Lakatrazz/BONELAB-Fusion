using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Representation;
using LabFusion.Utilities;
using static Il2CppMono.Security.X509.X520;

namespace LabFusion.Network {
    public static class NetworkUtilities {
        public static bool HasServer => FusionMod.CurrentNetworkLayer.IsServer || FusionMod.CurrentNetworkLayer.IsClient;
        public static bool IsServer => FusionMod.CurrentNetworkLayer.IsServer;
        public static bool IsClient => FusionMod.CurrentNetworkLayer.IsClient && !FusionMod.CurrentNetworkLayer.IsServer;

        public const string InvalidAvatarId = "BONELABFUSION.NONE";

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void SendServerMessage(byte userId, NetworkChannel channel, FusionMessage message) {
            if (FusionMod.CurrentNetworkLayer != null)
                FusionMod.CurrentNetworkLayer.SendServerMessage(userId, channel, message);
        }

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void SendServerMessage(ulong userId, NetworkChannel channel, FusionMessage message) {
            if (FusionMod.CurrentNetworkLayer != null)
                FusionMod.CurrentNetworkLayer.SendServerMessage(userId, channel, message);
        }


        /// <summary>
        /// Sends the message to the server if this is a client. Sends to all clients if this is a server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void BroadcastMessage(NetworkChannel channel, FusionMessage message) {
            if (FusionMod.CurrentNetworkLayer != null)
                FusionMod.CurrentNetworkLayer.BroadcastMessage(channel, message);
        }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void BroadcastMessageExcept(byte userId, NetworkChannel channel, FusionMessage message) {
            if (FusionMod.CurrentNetworkLayer != null)
                FusionMod.CurrentNetworkLayer.BroadcastMessageExcept(userId, channel, message);
        }

        public static void RemoveUser(ulong longId) {
            var id = PlayerId.GetPlayerId(longId);

            if (id != null) {
                if (PlayerRep.Representations.ContainsKey(id.SmallId))
                    PlayerRep.Representations[id.SmallId].Dispose();

                id.Dispose();

#if DEBUG
                FusionLogger.Log($"User with long id {longId} was removed.");
#endif
            }
        }

        public static void RemoveAllUsers() {
            foreach (var id in PlayerId.PlayerIds.ToList()) {
                RemoveUser(id.LongId);
            }
        }

        public static void OnDisconnect() {
            RemoveAllUsers();
        }
    }
}
