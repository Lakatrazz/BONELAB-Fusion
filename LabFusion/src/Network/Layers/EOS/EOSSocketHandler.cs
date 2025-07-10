using Epic.OnlineServices;
using LabFusion.Utilities;

namespace LabFusion.Network;

internal static class EOSSocketHandler
{
    private struct FragmentCollection
    {
        public byte[][] Fragments;
        public int ReceivedCount;
        public int TotalSize;
        public DateTime LastReceived;
    }

    private static readonly int MAX_EOS_PACKET_SIZE = 1170;

    private static readonly Dictionary<(string, ushort), FragmentCollection> _incomingFragments = new();

    internal static Epic.OnlineServices.P2P.SocketId SocketId = new Epic.OnlineServices.P2P.SocketId { SocketName = "FusionSocket" };

    internal static void ConfigureP2P()
    {
        Epic.OnlineServices.P2P.SetPortRangeOptions portOptions = new()
        {
            Port = 7777,
            MaxAdditionalPortsToTry = 5,
        };
        EOSManager.P2PInterface.SetPortRange(ref portOptions);

        // dont change unless we want to start leaking ips...
        Epic.OnlineServices.P2P.SetRelayControlOptions relayOptions = new()
        {
            RelayControl = Epic.OnlineServices.P2P.RelayControl.ForceRelays,
        };
        EOSManager.P2PInterface.SetRelayControl(ref relayOptions);
    }

    internal static void CloseConnections()
    {
        var closeConnectionsOptions = new Epic.OnlineServices.P2P.CloseConnectionsOptions
        {
            LocalUserId = EOSNetworkLayer.LocalUserId,
            SocketId = SocketId
        };

        EOSManager.P2PInterface.CloseConnections(ref closeConnectionsOptions);
    }

    internal static Result SendPacketToUser(ProductUserId userId, byte[] data, NetworkChannel channel, bool isServerHandled)
    {
        void HandleLocalMessage()
        {
            ReadableMessage readableMessage = new ReadableMessage()
            {
                Buffer = new ReadOnlySpan<byte>(data),
                IsServerHandled = isServerHandled
            };
            NativeMessageHandler.ReadMessage(readableMessage);
        }

        // Delay reading the message by two frames
        // Really dumb solution to fix the server sending its client settings before the client gets its connection response message
        if (userId == EOSNetworkLayer.LocalUserId)
        {
            DelayUtilities.InvokeDelayed(() => HandleLocalMessage(), 2);
            return Result.Success;
        }

        if (data.Length > MAX_EOS_PACKET_SIZE)
        {
            return SendFragmentedPacket(userId, data, channel, isServerHandled);
        }

        var reliability = channel == NetworkChannel.Reliable
            ? Epic.OnlineServices.P2P.PacketReliability.ReliableUnordered
            : Epic.OnlineServices.P2P.PacketReliability.UnreliableUnordered;

        var sendOptions = new Epic.OnlineServices.P2P.SendPacketOptions()
        {
            LocalUserId = EOSNetworkLayer.LocalUserId,
            RemoteUserId = userId,
            SocketId = SocketId,
            Channel = isServerHandled ? (byte)2 : (byte)1,
            Data = new ArraySegment<byte>(data),
            AllowDelayedDelivery = true,
            Reliability = reliability,
            DisableAutoAcceptConnection = false,
        };

        return EOSManager.P2PInterface.SendPacket(ref sendOptions);
    }

    private static Result SendFragmentedPacket(ProductUserId userId, byte[] data, NetworkChannel channel, bool isServerHandled)
    {
        int headerSize = 8;
        int maxDataPerFragment = MAX_EOS_PACKET_SIZE - headerSize;

        int totalFragments = (data.Length + maxDataPerFragment - 1) / maxDataPerFragment;

        if (totalFragments > 1000)
        {
            FusionLogger.Error($"Message too large: {data.Length} bytes would create {totalFragments} fragments");
            return Result.InvalidParameters;
        }

        ushort fragmentId = (ushort)Random.Shared.Next(ushort.MaxValue);

        for (int i = 0; i < totalFragments; i++)
        {
            int offset = i * maxDataPerFragment;
            int fragmentSize = System.Math.Min(maxDataPerFragment, data.Length - offset);

            byte[] fragmentPacket = new byte[headerSize + fragmentSize];

            BitConverter.GetBytes((ushort)0xF2A9).CopyTo(fragmentPacket, 0);
            BitConverter.GetBytes(fragmentId).CopyTo(fragmentPacket, 2);
            BitConverter.GetBytes((ushort)i).CopyTo(fragmentPacket, 4);
            BitConverter.GetBytes((ushort)totalFragments).CopyTo(fragmentPacket, 6);

            Array.Copy(data, offset, fragmentPacket, headerSize, fragmentSize);

            var reliability = channel == NetworkChannel.Reliable
                ? Epic.OnlineServices.P2P.PacketReliability.ReliableUnordered
                : Epic.OnlineServices.P2P.PacketReliability.UnreliableUnordered;

            var sendOptions = new Epic.OnlineServices.P2P.SendPacketOptions()
            {
                LocalUserId = EOSNetworkLayer.LocalUserId,
                RemoteUserId = userId,
                SocketId = SocketId,
                Channel = isServerHandled ? (byte)2 : (byte)1,
                Data = new ArraySegment<byte>(fragmentPacket),
                AllowDelayedDelivery = true,
                Reliability = reliability,
                DisableAutoAcceptConnection = false,
            };

            var result = EOSManager.P2PInterface.SendPacket(ref sendOptions);
            if (result != Result.Success)
            {
                FusionLogger.Error($"Failed to send fragment {i}/{totalFragments}: {result}");
                return result;
            }
        }

        return Result.Success;
    }

    internal static void ReceiveMessages()
    {
        try
        {
            for (int i = 0; i < 100; i++)
            {
                var getNextReceivedPacketSizeOptions = new Epic.OnlineServices.P2P.GetNextReceivedPacketSizeOptions
                {
                    LocalUserId = EOSNetworkLayer.LocalUserId,
                    RequestedChannel = null
                };

                if (EOSManager.P2PInterface.GetNextReceivedPacketSize(ref getNextReceivedPacketSizeOptions, out uint nextPacketSize) != Result.Success)
                {
                    break;
                }

                byte[] buffer = new byte[nextPacketSize];
                ArraySegment<byte> dataSegment = new ArraySegment<byte>(buffer);

                var receiveOptions = new Epic.OnlineServices.P2P.ReceivePacketOptions
                {
                    LocalUserId = EOSNetworkLayer.LocalUserId,
                    MaxDataSizeBytes = nextPacketSize,
                    RequestedChannel = null
                };

                ProductUserId peerId = null;

                Result result = EOSManager.P2PInterface.ReceivePacket(ref receiveOptions, ref peerId, ref SocketId, out byte channel, dataSegment, out uint bytesWritten);

                if (result == Result.Success && bytesWritten > 0)
                {
                    if (peerId != null)
                    {
                        NetworkInfo.LastReceivedUser = peerId.ToString();
                    }

                    if (HandleFragmentedPacket(buffer, (int)bytesWritten, peerId, out byte[] reassembledData))
                    {
                        var messageSpan = new ReadOnlySpan<byte>(reassembledData);

                        var readableMessage = new ReadableMessage()
                        {
                            Buffer = messageSpan,
                            IsServerHandled = channel == 2
                        };
                        NativeMessageHandler.ReadMessage(readableMessage);
                    }
                    else
                    {
                        bool isFragment = bytesWritten >= 8 && BitConverter.ToUInt16(buffer, 0) == 0xF2A9;

                        if (!isFragment)
                        {
                            var messageSpan = new ReadOnlySpan<byte>(buffer, 0, (int)bytesWritten);

                            var readableMessage = new ReadableMessage()
                            {
                                Buffer = messageSpan,
                                IsServerHandled = channel == 2
                            };
                            NativeMessageHandler.ReadMessage(readableMessage);
                        }
                    }
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

    private static bool HandleFragmentedPacket(byte[] buffer, int bytesWritten, ProductUserId peerId, out byte[] reassembledData)
    {
        reassembledData = null;

        if (bytesWritten < 8) return false;

        ushort magicMarker = BitConverter.ToUInt16(buffer, 0);
        if (magicMarker != 0xF2A9) return false;

        ushort fragmentId = BitConverter.ToUInt16(buffer, 2);
        ushort fragmentPart = BitConverter.ToUInt16(buffer, 4);
        ushort totalFragments = BitConverter.ToUInt16(buffer, 6);

        if (totalFragments == 0 || totalFragments > 1000)
        {
            FusionLogger.Error($"Invalid totalFragments: {totalFragments}");
            return false;
        }

        if (fragmentPart >= totalFragments)
        {
            FusionLogger.Error($"Fragment part {fragmentPart} >= total fragments {totalFragments}");
            return false;
        }

        var key = (peerId.ToString(), fragmentId);

        if (!_incomingFragments.TryGetValue(key, out var collection))
        {
            collection = new FragmentCollection
            {
                Fragments = new byte[totalFragments][],
                ReceivedCount = 0,
                TotalSize = 0,
                LastReceived = DateTime.UtcNow
            };
            _incomingFragments[key] = collection;
        }

        if (collection.Fragments.Length != totalFragments)
        {
            FusionLogger.Error($"Fragment collection length mismatch: {collection.Fragments.Length} != {totalFragments}");
            _incomingFragments.Remove(key);
            return false;
        }

        if (collection.Fragments[fragmentPart] == null)
        {
            int fragmentDataSize = bytesWritten - 8;
            if (fragmentDataSize < 0)
            {
                FusionLogger.Error($"Invalid fragment data size: {fragmentDataSize}");
                return false;
            }

            collection.Fragments[fragmentPart] = new byte[fragmentDataSize];
            Array.Copy(buffer, 8, collection.Fragments[fragmentPart], 0, fragmentDataSize);
            collection.ReceivedCount++;
            collection.TotalSize += fragmentDataSize;
            collection.LastReceived = DateTime.UtcNow;

            _incomingFragments[key] = collection;
        }

        if (collection.ReceivedCount == totalFragments)
        {
            try
            {
                reassembledData = new byte[collection.TotalSize];
                int offset = 0;

                for (int i = 0; i < totalFragments; i++)
                {
                    var fragment = collection.Fragments[i];
                    if (fragment == null)
                    {
                        FusionLogger.Error($"Missing fragment {i} during reassembly");
                        _incomingFragments.Remove(key);
                        return false;
                    }

                    if (offset + fragment.Length > reassembledData.Length)
                    {
                        FusionLogger.Error($"Fragment reassembly would overflow: {offset + fragment.Length} > {reassembledData.Length}");
                        _incomingFragments.Remove(key);
                        return false;
                    }

                    Array.Copy(fragment, 0, reassembledData, offset, fragment.Length);
                    offset += fragment.Length;
                }

                _incomingFragments.Remove(key);
                return true;
            }
            catch (Exception ex)
            {
                FusionLogger.LogException("Error during fragment reassembly", ex);
                _incomingFragments.Remove(key);
                return false;
            }
        }

        return false;
    }

    internal static void BroadcastToServer(NetworkChannel channel, NetMessage message)
    {
        Result result = SendPacketToUser(EOSNetworkLayer.HostId, message.ToByteArray(), channel, true);

        if (result != Result.Success)
        {
            // RETRY
            Result retry = SendPacketToUser(EOSNetworkLayer.HostId, message.ToByteArray(), channel, true);

            if (retry != Result.Success)
            {
                throw new Exception($"EOS result was {retry}.");
            }
        }
    }

    internal static void BroadcastToClients(NetworkChannel channel, NetMessage message)
    {
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

                Result result = SendPacketToUser(memberId, message.ToByteArray(), channel, false);
            }
        }
    }

    internal static void SendFromServer(string userId, NetworkChannel channel, NetMessage message)
    {
        if (EOSNetworkLayer.HostId != EOSNetworkLayer.LocalUserId)
        {
            FusionLogger.Error("SendFromServer can only be called by the server.");
            return;
        }

        ProductUserId targetUserId = ProductUserId.FromString(userId);
        Result result = SendPacketToUser(targetUserId, message.ToByteArray(), channel, false);
    }
}