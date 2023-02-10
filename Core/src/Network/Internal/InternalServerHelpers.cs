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
        private static void DisposeUser(PlayerId id) {
            if (id != null) {
                if (PlayerRepManager.TryGetPlayerRep(id.SmallId, out var rep))
                    rep.Dispose();

                id.Dispose();
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
            var id = new PlayerId(PlayerIdManager.LocalLongId, 0, GetInitialMetadata());
            id.Insert();
            PlayerIdManager.ApplyLocalId();

            // Register module message handlers so they can send messages
            var names = ModuleMessageHandler.GetExistingTypeNames();
            ModuleMessageHandler.PopulateHandlerTable(names);

            // Update hooks
            MultiplayerHooking.Internal_OnStartServer();

            // Send a notification
            FusionNotifier.Send("Started Server", "Started a server!", false, true);
        }

        /// <summary>
        /// Called when the user joins a server.
        /// </summary>
        internal static void OnJoinServer() {
            // Send settings
            FusionPreferences.SendClientSettings();

            // Update hooks
            MultiplayerHooking.Internal_OnJoinServer();

            // Send a notification
            FusionNotifier.Send("Joined Server", "Joined a server!", false, true);
        }

        /// <summary>
        /// Cleans up the scene from all users. ONLY call this from within a network layer!
        /// </summary>
        internal static void OnDisconnect(string reason = "") {
            // Cleanup information
            DisposeUsers();
            SyncManager.OnCleanup();
            ModuleMessageHandler.ClearHandlerTable();
            Physics.autoSimulation = true;

            // Cleanup prefs
            FusionPreferences.ReceivedServerSettings = null;

            // Update hooks
            MultiplayerHooking.Internal_OnDisconnect();

            // Send a notification
            if (string.IsNullOrWhiteSpace(reason)) {
                FusionNotifier.Send("Disconnected from Server", "Disconnected from the current server!", false, true);
            }
            else {
                FusionNotifier.Send("Disconnected from Server", $"You were disconnected for reason: {reason}", true, true);
            }
        }

        /// <summary>
        /// Updates information about the new user.
        /// </summary>
        /// <param name="id"></param>
        internal static void OnUserJoin(PlayerId id, bool isInitialJoin) {
            // Send client info
            FusionPreferences.SendClientSettings();

            // Update layer
            InternalLayerHelpers.OnUserJoin(id);

            // Update hooks
            MultiplayerHooking.Internal_OnPlayerJoin(id);

            // Send notification
            if (isInitialJoin && id.TryGetDisplayName(out var name)) {
                FusionNotifier.Send($"{name} Join", $"{name} joined the server.", false, true);
            }
        }

        /// <summary>
        /// Cleans up a single user after they have left.
        /// </summary>
        /// <param name="longId"></param>
        internal static void OnUserLeave(ulong longId) {
            var playerId = PlayerIdManager.GetPlayerId(longId);

            // Make sure the player exists in our game
            if (playerId == null)
                return;

            // Send notification
            if (playerId.TryGetDisplayName(out var name)) {
                FusionNotifier.Send($"{name} Leave", $"{name} left the server.", false, true);
            }

            MultiplayerHooking.Internal_OnPlayerLeave(playerId);

            DisposeUser(playerId);
        }

        /// <summary>
        /// Gets the default metadata for the local player.
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, string> GetInitialMetadata() {
            // Create the dict
            var metadata = new Dictionary<string, string> {
                // Username
                { MetadataHelper.UsernameKey, PlayerIdManager.LocalUsername },

                // Nickname
                { MetadataHelper.NicknameKey, PlayerIdManager.LocalNickname },

                // Permission
                { MetadataHelper.PermissionKey, NetworkInfo.IsServer ? PermissionLevel.OWNER.ToString() : PermissionLevel.DEFAULT.ToString() }
            };

            return metadata;
        }
    }
}
