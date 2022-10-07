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
        public static void SendMessageToServer(FusionMessage message)
        {
            try
            {
                // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
                int sizeOfMessage = message.Length;
                IntPtr intPtrMessage = System.Runtime.InteropServices.Marshal.AllocHGlobal(sizeOfMessage);
                System.Runtime.InteropServices.Marshal.Copy(message.Buffer, 0, intPtrMessage, sizeOfMessage);
                Result success = SteamNetworkLayer.SteamConnection.Connection.SendMessage(intPtrMessage, sizeOfMessage, SendType.Reliable);
                if (success == Result.OK)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                }
                else
                {
                    // RETRY
                    Result retry = SteamNetworkLayer.SteamConnection.Connection.SendMessage(intPtrMessage, sizeOfMessage, SendType.Reliable);
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(intPtrMessage); // Free up memory at pointer
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log("Unable to send message to socket server");
            }
        }

        public static void OnSocketMessageReceived(IntPtr messageIntPtr, int dataBlockSize) {
            try {
                byte[] message = new byte[dataBlockSize];
                System.Runtime.InteropServices.Marshal.Copy(messageIntPtr, message, 0, dataBlockSize);

                FusionMessageHandler.ReadMessage(message);
            }
            catch {
                FusionLogger.Log("Unable to process message from socket server!");
            }
        }
    }
}
