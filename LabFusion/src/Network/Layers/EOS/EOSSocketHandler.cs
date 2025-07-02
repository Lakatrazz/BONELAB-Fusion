using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using Il2CppSLZ.ModIO.WebSockets;
using LabFusion.Player;
using LabFusion.Utilities;
using Steamworks.Data;
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
            if (LocalUserId == userId)
            {
                FusionLogger.Error("Attempted to send a packet to self. This is not allowed.");
                return Result.UnexpectedError;
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
                DisableAutoAcceptConnection = false,
            };

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
            if (HostId == LocalUserId)
                return;

            Result result = SendPacketToUser(HostId, message.ToByteArray(), channel);

            if (result != Result.Success)
            {
                // RETRY
                Result retry = SendPacketToUser(HostId, message.ToByteArray(), channel);

                if (retry != Result.Success)
                {
                    throw new Exception($"EOS result was {retry}.");
                }
            }
        }

        internal static void BroadcastToClients(NetworkChannel channel, NetMessage message)
        {
            if (HostId != LocalUserId)
            {
                return;
            }
            if (NetworkLayerManager.Layer is EOSNetworkLayer layer)
            {
                var countOptions = new Epic.OnlineServices.Lobby.LobbyDetailsGetMemberCountOptions();
                uint memberCount = layer.LobbyDetails.GetMemberCount(ref countOptions);

                for (uint i = 0; i < memberCount; i++)
                {
                    var memberOptions = new Epic.OnlineServices.Lobby.LobbyDetailsGetMemberByIndexOptions
                    {
                        MemberIndex = i
                    };
                    ProductUserId memberId = layer.LobbyDetails.GetMemberByIndex(ref memberOptions);

                    if (memberId != LocalUserId)
                    {
                        Result result = SendPacketToUser(memberId, message.ToByteArray(), channel);
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
