using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Representation;
using LabFusion.Utilities;

using Steamworks;
using Steamworks.Data;

using UnityEngine;
using FusionHelper.Network;

namespace LabFusion.Network
{
    public static class ProxySocketHandler {
        /*public static SendType ConvertToSendType(NetworkChannel channel)
        {
            SendType sendType;
            switch (channel)
            {
                case NetworkChannel.Unreliable:
                default:
                    sendType = SendType.Unreliable;
                    break;
                case NetworkChannel.VoiceChat:
                    sendType = SendType.Unreliable | SendType.NoDelay;
                    break;
                case NetworkChannel.Reliable:
                    sendType = SendType.Reliable;
                    break;
            }
            return sendType;
        }*/

        /*public static void SendToClient(Connection connection, NetworkChannel channel, FusionMessage message)
        {
            SendType sendType = ConvertToSendType(channel);

            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = message.Length;
            IntPtr intPtrMessage = Marshal.AllocHGlobal(sizeOfMessage);
            Marshal.Copy(message.Buffer, 0, intPtrMessage, sizeOfMessage);

            connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);

            Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
        }*/

        public static void BroadcastToClients(NetworkChannel channel, FusionMessage message)
        {
            MessageTypes type = channel == NetworkChannel.Reliable ? MessageTypes.ReliableBroadcastToClients : MessageTypes.UnreliableBroadcastToClients;
            ProxyNetworkLayer.Instance.SendToProxyServer(message.Buffer, type);
        }

        public static void BroadcastToServer(NetworkChannel channel, FusionMessage message)
        {
            try
            {
                MessageTypes type = channel == NetworkChannel.Reliable ? MessageTypes.ReliableBroadcastToServer : MessageTypes.UnreliableBroadcastToServer;
                ProxyNetworkLayer.Instance.SendToProxyServer(message.Buffer, type);
            }
            catch (Exception e)
            {
                FusionLogger.Error($"Failed sending message to socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }

        public static void OnSocketMessageReceived(byte[] message, bool isServerHandled = false) {
            try {
                FusionMessageHandler.ReadMessage(message, isServerHandled);
            }
            catch (Exception e) {
                FusionLogger.Error($"Failed reading message from socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }
    }
}
