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
                case NetworkChannel.VoiceChat:
                    sendType = SendType.Unreliable | SendType.NoDelay;
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
            IntPtr intPtrMessage = Marshal.AllocHGlobal(sizeOfMessage);
            Marshal.Copy(message.Buffer, 0, intPtrMessage, sizeOfMessage);
            
            connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);

            Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
        }

        public static void BroadcastToClients(this SteamSocketManager socketManager, NetworkChannel channel, FusionMessage message) {
            SendType sendType = ConvertToSendType(channel);

            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = message.Length;
            IntPtr intPtrMessage = Marshal.AllocHGlobal(sizeOfMessage);
            Marshal.Copy(message.Buffer, 0, intPtrMessage, sizeOfMessage);

            for (var i = 0; i < socketManager.Connected.Count; i++) {
                var connection = socketManager.Connected[i];
                connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);
            }

            Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
        }

        public static void BroadcastToServer(NetworkChannel channel, FusionMessage message) {
            try
            {
                SendType sendType = ConvertToSendType(channel);

                // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
                int sizeOfMessage = message.Length;
                IntPtr intPtrMessage = Marshal.AllocHGlobal(sizeOfMessage);
                Marshal.Copy(message.Buffer, 0, intPtrMessage, sizeOfMessage);
                Result success = SteamNetworkLayer.SteamConnection.Connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);
                if (success == Result.OK) {
                    Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                }
                else {
                    // RETRY
                    Result retry = SteamNetworkLayer.SteamConnection.Connection.SendMessage(intPtrMessage, sizeOfMessage, sendType);
                    Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer

                    if (retry != Result.OK) {
                        throw new Exception($"Steam result was {retry}.");
                    }
                }
            }
            catch (Exception e) {
                FusionLogger.Error($"Failed sending message to socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }

        public static void OnSocketMessageReceived(IntPtr messageIntPtr, int dataBlockSize, bool isServerHandled = false) {
            try {
                byte[] message = ByteRetriever.Rent(dataBlockSize);
                Marshal.Copy(messageIntPtr, message, 0, dataBlockSize);

                FusionMessageHandler.ReadMessage(message, isServerHandled);

                ByteRetriever.Return(message);
            }
            catch (Exception e) {
                FusionLogger.Error($"Failed reading message from socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }
    }
}
