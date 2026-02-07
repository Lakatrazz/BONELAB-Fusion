using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

using LabFusion.Player;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Handles sending packets over EOS P2P. 
/// </summary>
internal class EOSPacketSender
{
    private const int MaxPacketSize = 1170;
    private const byte ServerChannel = 2;
    private const byte ClientChannel = 1;

    private readonly EOSBufferPool _bufferPool;
    private readonly FragmentSender _fragmentSender;
    private readonly SocketId _socketId;

    public EOSPacketSender(EOSBufferPool bufferPool, SocketId socketId)
    {
        _bufferPool = bufferPool;
        _socketId = socketId;
        _fragmentSender = new FragmentSender(bufferPool, MaxPacketSize);
    }

    public Result Send(ProductUserId remoteUserId, NetMessage message, NetworkChannel channel, bool isServerHandled)
    {
        if (remoteUserId == null)
            return Result.InvalidParameters;

        byte[] data = message.ToByteArray();
        byte targetChannel = isServerHandled ? ServerChannel : ClientChannel;

        return data.Length > MaxPacketSize
            ? _fragmentSender.SendFragmented(remoteUserId, data, channel, isServerHandled, _socketId, targetChannel)
            : SendSingle(remoteUserId, data, channel, targetChannel);
    }

    private Result SendSingle(ProductUserId remoteUserId, byte[] data, NetworkChannel channel, byte targetChannel)
    {
        var localUserId = ProductUserId.FromString(PlayerIDManager.LocalPlatformID);
        if (localUserId == null)
            return Result.InvalidState;

        var options = new SendPacketOptions
        {
            LocalUserId = localUserId,
            RemoteUserId = remoteUserId,
            SocketId = _socketId,
            Channel = targetChannel,
            Data = new ArraySegment<byte>(data),
            AllowDelayedDelivery = false,
            Reliability = GetReliability(channel),
            DisableAutoAcceptConnection = false
        };

        return EOSInterfaces.P2P.SendPacket(ref options);
    }

    private static PacketReliability GetReliability(NetworkChannel channel)
    {
        return channel == NetworkChannel.Reliable
            ? PacketReliability.ReliableUnordered
            : PacketReliability.UnreliableUnordered;
    }
}