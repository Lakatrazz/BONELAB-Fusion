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
            Result result;

            return result;
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
                        RequestedChannel = null
                    };

                    if (P2PInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSize) != Result.Success)
                    {
                        break;
                    }

                    if (nextPacketSize == 0)
                    {
                        continue;
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

            if ()
        }

        internal static void BroadcastToClients(NetworkChannel channel, NetMessage message)
        {

        }

        internal static void SendToClient(ProductUserId userId, NetworkChannel channel, NetMessage message)
        {
        }
    }
}
