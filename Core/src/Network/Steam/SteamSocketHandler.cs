using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabFusion.Data;
using LabFusion.Utilities;

using Steamworks;
using Steamworks.Data;

using UnityEngine;

namespace LabFusion.Network
{
    public static class SteamSocketHandler {
        public static SendType ConvertToSendType(NetworkChannel channel) {
            SendType sendType;
            switch (channel) {
                case NetworkChannel.Unreliable:
                default:
                    sendType = SendType.Unreliable;
                    break;
                case NetworkChannel.Reliable:
                    sendType = SendType.Reliable;
                    break;
            }
            return sendType;
        }

        public static void SendToClient(this SteamSocketManager socketManager, Connection connection, NetworkChannel channel, FusionMessage message) {
            SendType sendType = ConvertToSendType(channel);

            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = message.Length;
            IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);
            System.Runtime.InteropServices.Marshal.Copy(message.Buffer, 0, intPtrMessage, sizeOfMessage);

            connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);
        }

        public static void BroadcastToClients(this SteamSocketManager socketManager, NetworkChannel channel, FusionMessage message) {
            SendType sendType = ConvertToSendType(channel);

            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = message.Length;
            IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);
            System.Runtime.InteropServices.Marshal.Copy(message.Buffer, 0, intPtrMessage, sizeOfMessage);

            foreach (var connection in socketManager.Connected) {
                connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);
            }

            System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
        }

        public static void BroadcastToServer(NetworkChannel channel, FusionMessage message) {
            try
            {
                SendType sendType = ConvertToSendType(channel);

                // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
                int sizeOfMessage = message.Length;
                IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);
                System.Runtime.InteropServices.Marshal.Copy(message.Buffer, 0, intPtrMessage, sizeOfMessage);
                Result success = SteamNetworkLayer.SteamConnection.Connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);
                if (success == Result.OK) {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                }
                else {
                    // RETRY
                    Result retry = SteamNetworkLayer.SteamConnection.Connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                }
            }
            catch (Exception e) {
                FusionLogger.Error($"Failed sending message to socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }

        public static void OnSocketMessageReceived(IntPtr messageIntPtr, int dataBlockSize) {
            try {
                byte[] message = new byte[dataBlockSize];
                System.Runtime.InteropServices.Marshal.Copy(messageIntPtr, message, 0, dataBlockSize);

                FusionMessageHandler.ReadMessage(message);
            }
            catch (Exception e) {
                FusionLogger.Error($"Failed reading message from socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }
    }
}
