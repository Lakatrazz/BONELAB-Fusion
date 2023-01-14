using LabFusion.Representation;
using LabFusion.Syncables;
using LabFusion.Utilities;
using LabFusion.Preferences;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

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
        /// Initializes information about the server, such as module types.
        /// </summary>
        internal static void OnStartServer() {
            // Create local id
            var id = new PlayerId(PlayerIdManager.LocalLongId, 0, PlayerIdManager.LocalUsername);
            id.Insert();
            PlayerIdManager.ApplyLocalId();

            // Register module message handlers so they can send messages
            var names = ModuleMessageHandler.GetExistingTypeNames();
            ModuleMessageHandler.PopulateHandlerTable(names);

            // Update hooks
            MultiplayerHooking.Internal_OnStartServer();
        }

        /// <summary>
        /// Called when the user joins a server.
        /// </summary>
        internal static void OnJoinServer() {
            // Send settings
            FusionPreferences.SendClientSettings();

            // Update hooks
            MultiplayerHooking.Internal_OnJoinServer();
        }

        /// <summary>
        /// Cleans up the scene from all users. ONLY call this from within a network layer!
        /// </summary>
        internal static void OnDisconnect() {
            // Cleanup information
            DisposeUsers();
            SyncManager.OnCleanup();
            ModuleMessageHandler.ClearHandlerTable();
            Physics.autoSimulation = true;

            // Cleanup prefs
            FusionPreferences.ReceivedServerSettings = null;

            // Update hooks
            MultiplayerHooking.Internal_OnDisconnect();
        }

        /// <summary>
        /// Updates information about the new user.
        /// </summary>
        /// <param name="id"></param>
        internal static void OnUserJoin(PlayerId id) {
            // Send client info
            FusionPreferences.SendClientSettings();

            // Update layer
            InternalLayerHelpers.OnUserJoin(id);

            // Update hooks
            MultiplayerHooking.Internal_OnPlayerJoin(id);
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
