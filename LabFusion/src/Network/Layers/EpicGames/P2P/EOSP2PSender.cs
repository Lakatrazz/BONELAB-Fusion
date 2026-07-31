using System.Buffers;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using LabFusion.Utilities;

namespace LabFusion.Network;

internal class EOSP2PSender
{
    private const int MaxPacketSize = P2PInterface.MAX_PACKET_SIZE;
    private const int MaxDataPerFragment = MaxPacketSize - FragmentHeader.HeaderSize;
    private const byte ServerChannel = 2;
    private const byte ClientChannel = 1;

    internal EOSP2P P2P;
    private int _nextFragmentId = 1;
    
    internal EOSP2PSender(EOSP2P p2p)
    {
        P2P = p2p;
    }

    internal void Send(ProductUserId userId, NetMessage message, NetworkChannel channel, bool isServerHandled) => Send(userId, message.ToByteArray(), channel, isServerHandled);
    
    internal Result Send(ProductUserId remoteUserId, byte[] payload, NetworkChannel reliability, bool isServerHandled)
    {
        if (remoteUserId == null || payload == null)
            return Result.InvalidParameters;

        byte targetChannel = isServerHandled ? ServerChannel : ClientChannel;
        PacketReliability packetReliability = ConvertToPacketReliability(reliability);

        if (payload.Length + FragmentHeader.KindPrefixSize <= MaxPacketSize)
        {
            return SendSingle(remoteUserId, payload, packetReliability, targetChannel);
        }
        
        return SendFragmented(remoteUserId, payload, packetReliability, targetChannel);
    }

    private Result SendSingle(ProductUserId remoteUserId, byte[] payload, PacketReliability reliability, byte targetChannel)
    {
        int packetSize = payload.Length + FragmentHeader.KindPrefixSize;
        byte[] packetBuffer = ArrayPool<byte>.Shared.Rent(packetSize);

        try
        {
            packetBuffer[0] = FragmentHeader.KindSingle;
            Array.Copy(payload, 0, packetBuffer, FragmentHeader.KindPrefixSize, payload.Length);

            var options = new SendPacketOptions
            {
                LocalUserId = P2P.LocalUserId,
                RemoteUserId = remoteUserId,
                SocketId = P2P.SocketId,
                Channel = targetChannel,
                Data = new ArraySegment<byte>(packetBuffer, 0, packetSize),
                AllowDelayedDelivery = false,
                Reliability = reliability,
                DisableAutoAcceptConnection = false
            };

            return P2P.P2PInterface.SendPacket(ref options);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(packetBuffer);
        }
    }

    private Result SendFragmented(ProductUserId remoteUserId, byte[] payload, PacketReliability reliability, byte targetChannel)
    {
        int totalFragments = (payload.Length + MaxDataPerFragment - 1) / MaxDataPerFragment;

        if (totalFragments > 1000)
        {
            FusionLogger.Error($"Message size ({payload.Length} bytes) exceeds fragment limit ({totalFragments} fragments created). Discarding packet.");
            return Result.InvalidParameters;
        }

        ushort fragmentId = (ushort)(Interlocked.Increment(ref _nextFragmentId) & 0xFFFF);

        var sendPacketOptions = new SendPacketOptions
        {
            LocalUserId = P2P.LocalUserId,
            RemoteUserId = remoteUserId,
            SocketId = P2P.SocketId,
            Channel = targetChannel,
            AllowDelayedDelivery = false,
            Reliability = reliability,
            DisableAutoAcceptConnection = false
        };

        for (int i = 0; i < totalFragments; i++)
        {
            int offset = i * MaxDataPerFragment;
            int fragmentLength = System.Math.Min(MaxDataPerFragment, payload.Length - offset);
            int packetSize = FragmentHeader.HeaderSize + fragmentLength;

            byte[] packetBuffer = ArrayPool<byte>.Shared.Rent(packetSize);
            try
            {
                FragmentHeader.Write(packetBuffer.AsSpan(), fragmentId, (ushort)i, (ushort)totalFragments, payload.Length);
                Array.Copy(payload, offset, packetBuffer, FragmentHeader.HeaderSize, fragmentLength);

                sendPacketOptions.Data = new ArraySegment<byte>(packetBuffer, 0, packetSize);
                
                Result sendResult = P2P.P2PInterface.SendPacket(ref sendPacketOptions);
                if (sendResult != Result.Success)
                {
                    FusionLogger.Error($"Failed to send fragment {i}/{totalFragments}: {sendResult}");
                    return sendResult;
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(packetBuffer);
            }
        }

        return Result.Success;
    }

    private PacketReliability ConvertToPacketReliability(NetworkChannel channel)
    {
        return channel switch
        {
            NetworkChannel.Reliable => PacketReliability.ReliableOrdered,
            _ => PacketReliability.UnreliableUnordered,
        };
    }
}