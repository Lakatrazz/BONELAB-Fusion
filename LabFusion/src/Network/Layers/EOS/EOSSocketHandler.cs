using Epic.OnlineServices;
using Il2CppSLZ.ModIO.WebSockets;
using LabFusion.Player;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

using static LabFusion.Network.EOSNetworkLayer;

namespace LabFusion.Network
{
    internal static class EOSSocketHandler
    {
        internal static Epic.OnlineServices.P2P.SocketId SocketId = new Epic.OnlineServices.P2P.SocketId { SocketName = "FusionSocket" };

        internal static void ConfigureP2PSocketToAcceptConnections()
        {
            var portOptions = new Epic.OnlineServices.P2P.SetPortRangeOptions
            {
                Port = 7777,
                MaxAdditionalPortsToTry = 10
            };
            P2PInterface.SetPortRange(ref portOptions);

            var relayOptions = new Epic.OnlineServices.P2P.SetRelayControlOptions
            {
                RelayControl = Epic.OnlineServices.P2P.RelayControl.AllowRelays,
            };
            P2PInterface.SetRelayControl(ref relayOptions);
        }

        private static Result SendPacketToUser(ProductUserId userId, byte[] data, NetworkChannel channel)
        {
            if (LocalUserId == null || userId == null || data == null)
                return Result.InvalidParameters;

            if (LocalUserId == userId)
            {
                var messageSpan = new ReadOnlySpan<byte>(data);
                var readableMessage = new ReadableMessage()
                {
                    Buffer = messageSpan,
                    IsServerHandled = HostId == LocalUserId
                };
                NativeMessageHandler.ReadMessage(readableMessage);
                FusionLogger.Log($"Received message from self: {readableMessage.Buffer.ToArray().Length} bytes.");
                return Result.Success;
            }

            var reliability = channel == NetworkChannel.Reliable
                ? Epic.OnlineServices.P2P.PacketReliability.ReliableUnordered
                : Epic.OnlineServices.P2P.PacketReliability.UnreliableUnordered;

            var sendOptions = new Epic.OnlineServices.P2P.SendPacketOptions()
            {
                LocalUserId = LocalUserId,
                RemoteUserId = userId,
                SocketId = SocketId,
                Channel = 1,
                Data = new ArraySegment<byte>(data),
                AllowDelayedDelivery = true,
                Reliability = reliability,
                DisableAutoAcceptConnection = false
            };

            FusionLogger.Log($"Sending packet to user {userId} on channel {channel} with reliability {reliability}.");

            return P2PInterface.SendPacket(ref sendOptions);
        }

        internal static void ReceiveMessages()
        {
            if (LocalUserId == null)
                return;

            try
            {
                for (int i = 0; i < 100; i++)
                {
                    var getNextReceivedPacketSizeOptions = new Epic.OnlineServices.P2P.GetNextReceivedPacketSizeOptions
                    {
                        LocalUserId = LocalUserId,
                        RequestedChannel = 1
                    };

                    if (P2PInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSize) != Result.Success)
                    {
                        break;
                    }

                    byte[] buffer = new byte[nextPacketSize];
                    ArraySegment<byte> dataSegment = new ArraySegment<byte>(buffer);

                    var receiveOptions = new Epic.OnlineServices.P2P.ReceivePacketOptions
                    {
                        LocalUserId = LocalUserId,
                        MaxDataSizeBytes = nextPacketSize,
                        RequestedChannel = null
                    };

                    ProductUserId peerId = null;
                    byte channel = 1;

                    Result result = P2PInterface.ReceivePacket(ref receiveOptions, ref peerId, ref SocketId, out channel, dataSegment, out uint bytesWritten);

                    if (result == Result.Success && bytesWritten > 0)
                    {
                        if (peerId != null)
                        {
                            NetworkInfo.LastReceivedUser = peerId.ToString();
                        }

                        var messageSpan = new ReadOnlySpan<byte>(buffer, 0, (int)bytesWritten);

                        var readableMessage = new ReadableMessage()
                        {
                            Buffer = messageSpan,
                            IsServerHandled = HostId == LocalUserId
                        };

                        NativeMessageHandler.ReadMessage(readableMessage);
                    }
                    else if (result != Result.Success)
                    {
                        FusionLogger.Error($"Failed to receive packet: {result}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                FusionLogger.LogException("Error receiving P2P messages", ex);
            }
        }

        internal static void BroadcastToServer(NetworkChannel channel, NetMessage message)
        {
            Result result = SendPacketToUser(HostId, message.ToByteArray(), channel);

            switch (result)
            {
                case Result.Success:
                    FusionLogger.Log("Message sent to server successfully.");
                    break;
                case Result.InvalidParameters:
                    FusionLogger.Error("Invalid parameters when sending message to server.");
                    break;
                case Result.LimitExceeded:
                    FusionLogger.Warn("Message size exceeded the limit when sending to server or outgoing packet queue was full.");
                    break;
                case Result.NoConnection:
                    FusionLogger.Error("No connection to the server when trying to send message.");
                    break;
                default:
                    FusionLogger.Error($"Failed to send message to server with result: {result}");
                    break;
            }
        }

        internal static void BroadcastToClients(NetworkChannel channel, NetMessage message)
        {
            if (HostId != LocalUserId)
            {
                FusionLogger.Error("BroadcastToClients can only be called by the server.");
                return;
            }

            var countOptions = new Epic.OnlineServices.Lobby.LobbyDetailsGetMemberCountOptions();
            uint memberCount = LobbyDetails.GetMemberCount(ref countOptions);

            if (memberCount == 0)
            {
                FusionLogger.Warn("No members in the lobby to broadcast to.");
                return;
            }

            for (uint i = 0; i < memberCount; i++)
            {
                var memberOptions = new Epic.OnlineServices.Lobby.LobbyDetailsGetMemberByIndexOptions
                {
                    MemberIndex = i
                };
                ProductUserId memberId = LobbyDetails.GetMemberByIndex(ref memberOptions);

                if (memberId != LocalUserId)
                {
                    Result result = SendPacketToUser(memberId, message.ToByteArray(), channel);

                    switch (result)
                    {
                        case Result.Success:
                            FusionLogger.Log($"Message sent to client {memberId} successfully.");
                            break;
                        case Result.InvalidParameters:
                            FusionLogger.Error($"Invalid parameters when sending message to client {memberId}.");
                            break;
                        case Result.LimitExceeded:
                            FusionLogger.Warn($"Message size exceeded the limit when sending to client {memberId} or outgoing packet queue was full.");
                            break;
                        case Result.NoConnection:
                            FusionLogger.Error($"No connection to client {memberId} when trying to send message.");
                            break;
                        default:
                            FusionLogger.Error($"Failed to send message to client {memberId} with result: {result}");
                            break;
                    }
                }
            }
        }

        internal static void SendToClient(ProductUserId userId, NetworkChannel channel, NetMessage message)
        {
            if (HostId != LocalUserId)
            {
                FusionLogger.Error("SendToClient can only be called by the server.");
                return;
            }

            Result result = SendPacketToUser(userId, message.ToByteArray(), channel);

            switch (result)
            {
                case Result.Success:
                    FusionLogger.Log($"Message sent to client {userId} successfully.");
                    break;
                case Result.InvalidParameters:
                    FusionLogger.Error($"Invalid parameters when sending message to client {userId}.");
                    break;
                case Result.LimitExceeded:
                    FusionLogger.Warn($"Message size exceeded the limit when sending to client {userId} or outgoing packet queue was full.");
                    break;
                case Result.NoConnection:
                    FusionLogger.Error($"No connection to client {userId} when trying to send message.");
                    break;
                default:
                    FusionLogger.Error($"Failed to send message to client {userId} with result: {result}");
                    break;
            }
        }

        internal static void SendFromServer(string userId, NetworkChannel channel, NetMessage message)
        {
            if (HostId != LocalUserId)
            {
                FusionLogger.Error("SendFromServer can only be called by the server.");
                return;
            }

            ProductUserId targetUserId = ProductUserId.FromString(userId);
            SendToClient(targetUserId, channel, message);
        }
    }
}
