using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BoneLib.BoneMenu.Elements;

using LabFusion.Representation;

namespace LabFusion.Network
{
    /// <summary>
    /// Privacy type for a server.
    /// </summary>
    public enum ServerPrivacy {
        PUBLIC = 0,
        PRIVATE = 1,
        FRIENDS_ONLY = 2,
        LOCKED = 3,
    }

    /// <summary>
    /// The foundational class for a server's networking system.
    /// </summary>
    public abstract class NetworkLayer {
        /// <summary>
        /// Returns true if this layer is hosting a server.
        /// </summary>
        internal virtual bool IsServer => false;

        /// <summary>
        /// Returns true if this layer is a client inside of a server (still returns true if this is the host!)
        /// </summary>
        internal virtual bool IsClient => false;

        /// <summary>
        /// Returns true if the networking solution allows the server to send messages to the host (Actual Server Logic vs P2P).
        /// </summary>
        internal virtual bool ServerCanSendToHost => true;

        /// <summary>
        /// Returns the current active lobby.
        /// </summary>
        internal virtual INetworkLobby CurrentLobby => null;

        /// <summary>
        /// Starts the server.
        /// </summary>
        internal abstract void StartServer();

        /// <summary>
        /// Disconnects the client from the connection and/or server.
        /// </summary>
        internal abstract void Disconnect(string reason = "");

        /// <summary>
        /// Returns the username of the player with id userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal virtual string GetUsername(ulong userId) => "Unknown";

        /// <summary>
        /// Returns true if this is a friend (ex. steam friends).
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        internal virtual bool IsFriend(ulong userId) => false;

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// Sends the message to the dedicated server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void SendToServer(NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// Sends the message to the server if this is a client. Sends to all clients if this is a server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void BroadcastMessage(NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void BroadcastMessageExcept(byte userId, NetworkChannel channel, FusionMessage message, bool ignoreHost = true) {
            for (var i = 0; i < PlayerIdManager.PlayerIds.Count; i++) {
                var id = PlayerIdManager.PlayerIds[i];

                if (id.SmallId != userId && (id.SmallId != 0 || !ignoreHost))
                    SendFromServer(id.SmallId, channel, message);
            }
        }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        internal virtual void BroadcastMessageExcept(ulong userId, NetworkChannel channel, FusionMessage message, bool ignoreHost = true)
        {
            for (var i = 0; i < PlayerIdManager.PlayerIds.Count; i++) {
                var id = PlayerIdManager.PlayerIds[i];
                if (id.LongId != userId && (id.SmallId != 0 || !ignoreHost))
                    SendFromServer(id.SmallId, channel, message);
            }
        }

        internal abstract void OnInitializeLayer();

        internal virtual void OnLateInitializeLayer() { }

        internal abstract void OnCleanupLayer();

        internal virtual void OnUpdateLayer() { }

        internal virtual void OnLateUpdateLayer() { }

        internal virtual void OnGUILayer() { }

        internal virtual void OnVoiceChatUpdate() { }

        internal virtual void OnVoiceBytesReceived(PlayerId id, byte[] bytes) { }

        internal virtual void OnUserJoin(PlayerId id) { }

        internal virtual void OnSetupBoneMenu(MenuCategory category) { }
    }
}
