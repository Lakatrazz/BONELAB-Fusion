using LabFusion.Representation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public abstract class NetworkLayer {
        public virtual bool IsServer => false;

        public virtual bool IsClient => false;

        public abstract void StartServer();

        public abstract void Disconnect();

        /// <summary>
        /// Returns the username of the player with id userId.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public virtual string GetUsername(ulong userId) => "Unknown";

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public virtual void SendServerMessage(byte userId, NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public virtual void SendServerMessage(ulong userId, NetworkChannel channel, FusionMessage message) { }


        /// <summary>
        /// Sends the message to the server if this is a client. Sends to all clients if this is a server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public virtual void BroadcastMessage(NetworkChannel channel, FusionMessage message) { }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public virtual void BroadcastMessageExcept(byte userId, NetworkChannel  channel, FusionMessage message) {
            foreach (var id in PlayerId.PlayerIds) {
                if (id.SmallId != userId)
                    SendServerMessage(id.SmallId, channel, message);
            }
        }

        public abstract void OnInitializeLayer();

        public virtual void OnLateInitializeLayer() { }

        public abstract void OnCleanupLayer();

        public virtual void OnUpdateLayer() { }

        public virtual void OnLateUpdateLayer() { }

        public virtual void OnGUILayer() { }
    }
}
