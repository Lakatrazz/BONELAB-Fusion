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
        public static void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message)
        {
            if (message == null)
                return;

            if (NetworkInfo.CurrentNetworkLayer != null) {
                NetworkInfo.BytesUp += message.Buffer.Length;

                NetworkInfo.CurrentNetworkLayer.SendFromServer(userId, channel, message);
            }
        }

        /// <summary>
        /// Sends the message to the specified user if this is a server.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message)
        {
            if (message == null)
                return;

            if (NetworkInfo.CurrentNetworkLayer != null) {
                NetworkInfo.BytesUp += message.Buffer.Length;

                NetworkInfo.CurrentNetworkLayer.SendFromServer(userId, channel, message);
            }
        }

        /// <summary>
        /// Sends the message to the dedicated server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void SendToServer(NetworkChannel channel, FusionMessage message) {
            if (message == null)
                return;

            if (NetworkInfo.CurrentNetworkLayer != null) {
                NetworkInfo.BytesUp += message.Buffer.Length;

                if (!NetworkInfo.IsServer)
                    NetworkInfo.CurrentNetworkLayer.SendToServer(channel, message);
                else
                    FusionMessageHandler.ReadMessage(message.Buffer, true);
            }
        }

        /// <summary>
        /// Sends the message to the server if this is a client. Sends to all clients if this is a server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void BroadcastMessage(NetworkChannel channel, FusionMessage message)
        {
            if (message == null)
                return;

            if (NetworkInfo.CurrentNetworkLayer != null) {
                NetworkInfo.BytesUp += message.Buffer.Length;

                NetworkInfo.CurrentNetworkLayer.BroadcastMessage(channel, message);

                // Backup incase the message cannot be sent to the host, which this targets.
                if (!NetworkInfo.ServerCanSendToHost && NetworkInfo.IsServer) {
                    FusionMessageHandler.ReadMessage(message.Buffer, false);
                }
            }
        }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void BroadcastMessageExcept(byte userId, NetworkChannel channel, FusionMessage message, bool ignoreHost = true)
        {
            if (message == null)
                return;

            if (NetworkInfo.CurrentNetworkLayer != null) {
                NetworkInfo.BytesUp += message.Buffer.Length;

                NetworkInfo.CurrentNetworkLayer.BroadcastMessageExcept(userId, channel, message, ignoreHost);

                // Backup incase the message cannot be sent to the host, which this targets.
                if (!ignoreHost && userId != PlayerIdManager.LocalSmallId && !NetworkInfo.ServerCanSendToHost && NetworkInfo.IsServer) {
                    FusionMessageHandler.ReadMessage(message.Buffer, false);
                }
            }
        }

        /// <summary>
        /// If this is a server, sends this message back to all users except for the provided id.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void BroadcastMessageExcept(ulong userId, NetworkChannel channel, FusionMessage message, bool ignoreHost = true)
        {
            if (message == null)
                return;

            if (NetworkInfo.CurrentNetworkLayer != null) {
                NetworkInfo.BytesUp += message.Buffer.Length;

                NetworkInfo.CurrentNetworkLayer.BroadcastMessageExcept(userId, channel, message, ignoreHost);

                // Backup incase the message cannot be sent to the host, which this targets.
                if (!ignoreHost && userId != PlayerIdManager.LocalLongId && !NetworkInfo.ServerCanSendToHost && NetworkInfo.IsServer) {
                    FusionMessageHandler.ReadMessage(message.Buffer, false);
                }
            }
        }

        /// <summary>
        /// Sends the message to the server if this is a client. Sends to all clients EXCEPT THE HOST if this is a server.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        public static void BroadcastMessageExceptSelf(NetworkChannel channel, FusionMessage message) {
            if (message == null)
                return;

            if (NetworkInfo.IsServer) {
                BroadcastMessageExcept(0, channel, message);
            }
            else {
                BroadcastMessage(channel, message);
            }
        }
    }
}
