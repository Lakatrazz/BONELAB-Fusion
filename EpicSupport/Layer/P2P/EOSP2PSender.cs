using System.Buffers;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using LabFusion.Network;

namespace MarrowFusion.Epic;

internal class EOSP2PSender
{
    internal EOSP2P P2P;
    
    private readonly Dictionary<ClientPlatformID, ProductUserId> productUserIdCache = new();
    
    internal EOSP2PSender(EOSP2P p2p)
    {
        P2P = p2p;
    }
    
    internal void SendToClient(NetMessage message, NetworkChannel channel, ClientPlatformID clientPlatformID)
    {
        var remoteUserId = GetProductUserId(clientPlatformID);

        Send(message, channel, remoteUserId, false);
    }
    
    internal void SendToClients(NetMessage message, NetworkChannel channel, ReadOnlySpan<ClientPlatformID> clientPlatformIDs)
    {
        foreach (var clientPlatformID in clientPlatformIDs)
        {
            SendToClient(message, channel, clientPlatformID);
        }
    }

    internal void SendToServer(NetMessage message, NetworkChannel channel)
    {
        Send(message, channel, P2P.Client.ConnectedUserId, true);
    }

    private void Send(NetMessage message, NetworkChannel channel, ProductUserId remoteUserId, bool isServerHandled)
    {
        byte[] rented = ArrayPool<byte>.Shared.Rent(message.Length);

        try
        {
            CopyInto(message, rented);

            SendRaw(new ArraySegment<byte>(rented, 0, message.Length), channel, remoteUserId, isServerHandled);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private unsafe void CopyInto(NetMessage message, byte[] destination)
    {
        fixed (byte* destinationPtr = destination)
        {
            Buffer.MemoryCopy(message.Buffer, destinationPtr, destination.Length, message.Length);
        }
    }

    private void SendRaw(ArraySegment<byte> data, NetworkChannel channel, ProductUserId remoteUserId, bool isServerHandled)
    {
        if (remoteUserId == null || !remoteUserId.IsValid())
        {
            return;
        }
        
        byte targetChannel = GetChannel(channel, isServerHandled);
        PacketReliability reliability = ToPacketReliability(channel);

        foreach (var packet in P2P.Fragmenter.Fragment(data))
        {
            var sendOptions = new SendPacketOptions
            {
                LocalUserId = P2P.LocalUserId,
                RemoteUserId = remoteUserId,
                SocketId = P2P.SocketId,
                Channel = targetChannel,
                Data = packet,
                AllowDelayedDelivery = true,
                Reliability = reliability,
            };

            var result = P2P.P2PInterface.SendPacket(ref sendOptions);
            if (result != Result.Success)
            {
                EpicModule.Logger.Warn($"Failed to send EOS P2P packet: {result}");
            }
        }
    }

    // Used for connecting
    internal void SendEmpty(ProductUserId remoteUserId)
    {
        if (remoteUserId == null || !remoteUserId.IsValid())
        {
            return;
        }
        
        var sendOptions = new SendPacketOptions
        {
            LocalUserId = P2P.LocalUserId,
            RemoteUserId = remoteUserId,
            SocketId = P2P.SocketId,
            Channel = 0,
            Data = new ArraySegment<byte>(Array.Empty<byte>()),
            AllowDelayedDelivery = true,
            Reliability = PacketReliability.ReliableOrdered
        };

        P2P.P2PInterface.SendPacket(ref sendOptions);
    }
    
    private static byte GetChannel(NetworkChannel channel, bool isServerHandled)
    {
        return (isServerHandled, channel) switch
        {
            (true, NetworkChannel.Reliable) => EOSP2P.ServerReliableChannel,
            (true, NetworkChannel.Unreliable) => EOSP2P.ServerUnreliableChannel,
            (false, NetworkChannel.Reliable) => EOSP2P.ClientReliableChannel,
            (false, NetworkChannel.Unreliable) => EOSP2P.ClientUnreliableChannel,
            _ => throw new ArgumentOutOfRangeException(nameof(channel)),
        };
    }
    
    private static PacketReliability ToPacketReliability(NetworkChannel channel)
    {
        return channel switch
        {
            NetworkChannel.Reliable => PacketReliability.ReliableOrdered,
            NetworkChannel.Unreliable => PacketReliability.UnreliableUnordered,
            _ => throw new ArgumentOutOfRangeException(nameof(channel)),
        };
    }
    
    private ProductUserId GetProductUserId(ClientPlatformID clientPlatformID)
    {
        if (!productUserIdCache.TryGetValue(clientPlatformID, out var userId))
        {
            userId = ProductUserId.FromString(clientPlatformID.Value);
            productUserIdCache[clientPlatformID] = userId;
        }
        
        return userId;
    }
}