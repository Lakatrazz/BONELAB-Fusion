using System.Buffers;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using LabFusion.Network;

namespace MarrowFusion.Epic;

internal class EOSP2PReceiver
{
    internal EOSP2P P2P;
    
    private readonly Dictionary<ProductUserId, ClientPlatformID> platformIdCache = new();

    internal EOSP2PReceiver(EOSP2P p2p)
    {
        P2P = p2p;
    }

    internal void Tick()
    {
        const int maxPacketsPerTick = 64;

        int processed = 0;
        while (processed < maxPacketsPerTick && TryReceiveNextPacket())
        {
            processed++;
        }
    }

    private ProductUserId peerId;
    private SocketId socketId;
    private bool TryReceiveNextPacket()
    {
        var sizeOptions = new GetNextReceivedPacketSizeOptions
        {
            LocalUserId = P2P.LocalUserId,
        };

        var sizeResult = P2P.P2PInterface.GetNextReceivedPacketSize(ref sizeOptions, out uint packetSize);
        if (sizeResult != Result.Success)
        {
            return false;
        }

        byte[] rented = ArrayPool<byte>.Shared.Rent((int)packetSize);

        try
        {
            var receiveOptions = new ReceivePacketOptions
            {
                LocalUserId = P2P.LocalUserId,
                MaxDataSizeBytes = packetSize,
            };

            var receiveResult = P2P.P2PInterface.ReceivePacket(ref receiveOptions, ref peerId, ref socketId, out byte channel, rented, out uint bytesWritten);
            if (receiveResult != Result.Success)
            {
                return false;
            }
            
            // Was added for the sake of the connection packet
            if (bytesWritten == 0)
            {
                return true;
            }
            
            HandlePacket(peerId, rented, (int)bytesWritten, channel);

            return true;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private void HandlePacket(ProductUserId senderId, byte[] data, int length, byte channel)
    {
        var rawData = new ArraySegment<byte>(data, 0, length);

        if (!P2P.Fragmenter.ProcessIncoming(senderId, rawData, channel, out var payload))
        {
            // We are still waiting for more fragments
            return;
        }
        
        switch (channel)
        {
            case EOSP2P.ServerReliableChannel:
            case EOSP2P.ServerUnreliableChannel:
                MessageManager.ReadMessageOnServer(payload, GetPlatformID(senderId));
                break;
            case EOSP2P.ClientReliableChannel:
            case EOSP2P.ClientUnreliableChannel:
                MessageManager.ReadMessageOnClient(payload);
                break;
            default:
                EpicModule.Logger.Warn($"Message received on unknown channel: {channel}");
                break;
        }
    }
    
    private ClientPlatformID GetPlatformID(ProductUserId userId)
    {
        if (!platformIdCache.TryGetValue(userId, out var platformId))
        {
            platformId = new ClientPlatformID(userId.ToString());
            platformIdCache[userId] = platformId;
        }
        
        return platformId;
    }
}