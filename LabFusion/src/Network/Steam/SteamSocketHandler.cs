﻿using System;
using LabFusion.Utilities;
using Steamworks;
using Steamworks.Data;

namespace LabFusion.Network
{
    public static class SteamSocketHandler
    {
        public static SendType ConvertToSendType(NetworkChannel channel)
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
        }

        public static void SendToClient(this SteamSocketManager socketManager, Connection connection, NetworkChannel channel, FusionMessage message)
        {
            SendType sendType = ConvertToSendType(channel);
            int sizeOfMessage = message.Length;

            unsafe
            {
                connection.SendMessage((IntPtr)message.Buffer, sizeOfMessage, sendType);
            }
        }

        public static void BroadcastToClients(this SteamSocketManager socketManager, NetworkChannel channel, FusionMessage message)
        {
            SendType sendType = ConvertToSendType(channel);

            // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
            int sizeOfMessage = message.Length;

            unsafe
            {
                IntPtr messagePtr = (IntPtr)message.Buffer;

                for (var i = 0; i < socketManager.Connected.Count; i++)
                {
                    var connection = socketManager.Connected[i];
                    connection.SendMessage(messagePtr, sizeOfMessage, sendType);
                }
            }
        }

        public static void BroadcastToServer(NetworkChannel channel, FusionMessage message)
        {
            try
            {
                SendType sendType = ConvertToSendType(channel);

                // Convert string/byte[] message into IntPtr data type for efficient message send / garbage management
                int sizeOfMessage = message.Length;

                unsafe
                {
                    IntPtr messagePtr = (IntPtr)message.Buffer;
                    Connection connection = SteamNetworkLayer.SteamConnection.Connection;

                    Result success = connection.SendMessage(messagePtr, sizeOfMessage, sendType);
                    if (success != Result.OK)
                    {
                        // RETRY
                        Result retry = connection.SendMessage(messagePtr, sizeOfMessage, sendType);

                        if (retry != Result.OK)
                        {
                            throw new Exception($"Steam result was {retry}.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                FusionLogger.Error($"Failed sending message to socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }

        public static void OnSocketMessageReceived(IntPtr messageIntPtr, int dataBlockSize, bool isServerHandled = false)
        {
            try
            {
                unsafe
                {
                    FusionMessageHandler.ReadMessage((byte*)messageIntPtr, dataBlockSize, isServerHandled);
                }
            }
            catch (Exception e)
            {
                FusionLogger.Error($"Failed reading message from socket server with reason: {e.Message}\nTrace:{e.StackTrace}");
            }
        }
    }
}
