using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Representation;
using LabFusion.Utilities;

namespace LabFusion.Network
{
    /// <summary>
    /// Helper class for sending messages to the server, or to other users if this is the server.
    /// </summary>
    public static class MessageSender
    {
        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void SendServerMessage(byte userId, NetworkChannel channel, FusionMessage message)
        {
            if (NetworkInfo.CurrentNetworkLayer != null)
                NetworkInfo.CurrentNetworkLayer.SendServerMessage(userId, channel, message);
        }

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void SendServerMessage(ulong userId, NetworkChannel channel, FusionMessage message)
        {
            if (NetworkInfo.CurrentNetworkLayer != null)
                NetworkInfo.CurrentNetworkLayer.SendServerMessage(userId, channel, message);
        }

        /// <summary>
        /// Sends the message to the server if this is a client. Sends to all clients if this is a server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void BroadcastMessage(NetworkChannel channel, FusionMessage message)
        {
            if (NetworkInfo.CurrentNetworkLayer != null)
                NetworkInfo.CurrentNetworkLayer.BroadcastMessage(channel, message);
        }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void BroadcastMessageExcept(byte userId, NetworkChannel channel, FusionMessage message)
        {
            if (NetworkInfo.CurrentNetworkLayer != null)
                NetworkInfo.CurrentNetworkLayer.BroadcastMessageExcept(userId, channel, message);
        }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void BroadcastMessageExcept(ulong userId, NetworkChannel channel, FusionMessage message)
        {
            if (NetworkInfo.CurrentNetworkLayer != null)
                NetworkInfo.CurrentNetworkLayer.BroadcastMessageExcept(userId, channel, message);
        }
    }
}
