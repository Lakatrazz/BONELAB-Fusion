using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Network
{
    public abstract class NetworkLayer {
        public virtual bool IsServer => false;

        public abstract void StartServer();

        public abstract void Disconnect();

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

        public abstract void OnInitializeLayer();

        public virtual void OnLateInitializeLayer() { }

        public abstract void OnCleanupLayer();

        public virtual void OnUpdateLayer() { }

        public virtual void OnLateUpdateLayer() { }

        public virtual void OnGUILayer() { }
    }
}
