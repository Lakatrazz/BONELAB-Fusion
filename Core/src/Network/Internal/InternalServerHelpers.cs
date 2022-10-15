using LabFusion.Representation;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network {
    /// <summary>
    /// Internal class used for cleaning up servers, executing events on disconnect, etc.
    /// </summary>
    internal static class InternalServerHelpers {
        private static void DisposeUser(ulong longId) => DisposeUser(PlayerIdManager.GetPlayerId(longId));

        private static void DisposeUser(byte smallId) => DisposeUser(PlayerIdManager.GetPlayerId(smallId));

        private static void DisposeUser(PlayerId id) {
            if (id != null) {
                if (PlayerRep.Representations.ContainsKey(id.SmallId))
                    PlayerRep.Representations[id.SmallId].Dispose();

                id.Dispose();

#if DEBUG
                FusionLogger.Log($"User with long id {id.LongId} was removed.");
#endif
            }
        }

        private static void DisposeUsers() {
            foreach (var id in PlayerIdManager.PlayerIds.ToList()) {
                DisposeUser(id);
            }
        }

        /// <summary>
        /// Cleans up the scene from all users. ONLY call this from within a network layer!
        /// </summary>
        internal static void OnDisconnect() {
            DisposeUsers();
        }

        /// <summary>
        /// Cleans up a single user after they have left.
        /// </summary>
        /// <param name="longId"></param>
        internal static void OnUserLeave(ulong longId) {
            DisposeUser(longId);
        }
    }
}
