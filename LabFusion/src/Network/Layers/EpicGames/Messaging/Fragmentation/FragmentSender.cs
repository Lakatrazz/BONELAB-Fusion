using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Handles sending fragmented messages over EOS P2P.
/// </summary>
internal class FragmentSender
{
    private const int MaxFragments = 1000;

    private readonly EOSBufferPool _bufferPool;
    private readonly int _maxPacketSize;
    private int _nextFragmentId = 1;

    public FragmentSender(EOSBufferPool bufferPool, int maxPacketSize)
    {
        _bufferPool = bufferPool;
        _maxPacketSize = maxPacketSize;
    }

    public Result SendFragmented(
        ProductUserId remoteUserId,
        byte[] data,
        NetworkChannel channel,
        bool isServerHandled,
        SocketId socketId,
        byte targetChannel)
    {
        int maxDataPerFragment = _maxPacketSize - FragmentHeader.Size;
        int totalFragments = (data.Length + maxDataPerFragment - 1) / maxDataPerFragment;

        if (totalFragments > MaxFragments)
        {
            FusionLogger.Error($"Message too large:  {data.Length} bytes would create {totalFragments} fragments");
            return Result.InvalidParameters;
        }

        var localUserId = ProductUserId.FromString(PlayerIDManager.LocalPlatformID);
        if (localUserId == null)
            return Result.InvalidState;

        ushort fragmentId = (ushort)(Interlocked.Increment(ref _nextFragmentId) & 0xFFFF);

        var baseOptions = new SendPacketOptions
        {
            LocalUserId = localUserId,
            RemoteUserId = remoteUserId,
            SocketId = socketId,
            Channel = targetChannel,
            AllowDelayedDelivery = false,
            Reliability = GetReliability(channel),
            DisableAutoAcceptConnection = false
        };

        for (int i = 0; i < totalFragments; i++)
        {
            var result = SendFragment(ref baseOptions, data, fragmentId, i, totalFragments, maxDataPerFragment);
            if (result != Result.Success)
            {
                FusionLogger.Error($"Failed to send fragment {i + 1}/{totalFragments}: {result}");
                return result;
            }
        }

        return Result.Success;
    }

    private Result SendFragment(
        ref SendPacketOptions options,
        byte[] data,
        ushort fragmentId,
        int fragmentIndex,
        int totalFragments,
        int maxDataPerFragment)
    {
        int offset = fragmentIndex * maxDataPerFragment;
        int fragmentSize = System.Math.Min(maxDataPerFragment, data.Length - offset);
        int packetSize = FragmentHeader.Size + fragmentSize;

        var packet = _bufferPool.Rent(packetSize);
        try
        {
            FragmentHeader.Write(packet.AsSpan(), fragmentId, (ushort)fragmentIndex, (ushort)totalFragments);
            Array.Copy(data, offset, packet, FragmentHeader.Size, fragmentSize);

            options.Data = new ArraySegment<byte>(packet, 0, packetSize);
            return EOSInterfaces.P2P.SendPacket(ref options);
        }
        finally
        {
            _bufferPool.Return(packet);
        }
    }

    private static PacketReliability GetReliability(NetworkChannel channel)
    {
        return channel == NetworkChannel.Reliable
            ? PacketReliability.ReliableUnordered
            : PacketReliability.UnreliableUnordered;
    }
}