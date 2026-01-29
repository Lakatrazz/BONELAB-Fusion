using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

using LabFusion.Player;
using LabFusion.Utilities;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Handles receiving packets from EOS P2P.
/// </summary>
internal class EOSPacketReceiver
{
    private const int MaxMessagesPerFrame = 100;
    private const byte ServerChannel = 2;

    private readonly EOSBufferPool _bufferPool;
    private readonly FragmentReceiver _fragmentReceiver;
    private readonly SocketId _socketId;

    public EOSPacketReceiver(EOSBufferPool bufferPool, SocketId socketId)
    {
        _bufferPool = bufferPool;
        _socketId = socketId;
        _fragmentReceiver = new FragmentReceiver();
    }

    public void ReceiveMessages()
    {
        _fragmentReceiver.CleanupIfNeeded();

        try
        {
            var localUserId = ProductUserId.FromString(PlayerIDManager.LocalPlatformID);
            if (localUserId == null)
                return;

            var getPacketSizeOptions = new GetNextReceivedPacketSizeOptions
            {
                LocalUserId = localUserId,
                RequestedChannel = null
            };

            var receiveOptions = new ReceivePacketOptions
            {
                LocalUserId = localUserId,
                RequestedChannel = null
            };

            for (int i = 0; i < MaxMessagesPerFrame; i++)
            {
                if (!TryReceivePacket(ref getPacketSizeOptions, ref receiveOptions, out var packetData))
                    break;

                ProcessPacket(packetData);
            }
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("receiving P2P messages", ex);
        }
    }

    private bool TryReceivePacket(
        ref GetNextReceivedPacketSizeOptions sizeOptions,
        ref ReceivePacketOptions receiveOptions,
        out ReceivedPacket packet)
    {
        packet = default;

        if (EOSInterfaces.P2P == null)
            return false;

        if (EOSInterfaces.P2P.GetNextReceivedPacketSize(ref sizeOptions, out uint packetSize) != Result.Success)
            return false;

        if (packetSize == 0)
            return false;

        var buffer = _bufferPool.Rent((int)packetSize);
        var dataSegment = new ArraySegment<byte>(buffer, 0, (int)packetSize);
        receiveOptions.MaxDataSizeBytes = packetSize;

        ProductUserId peerId = null;
        SocketId socketId = _socketId;

        var result = EOSInterfaces.P2P.ReceivePacket(
            ref receiveOptions,
            ref peerId,
            ref socketId,
            out byte channel,
            dataSegment,
            out uint bytesWritten);

        if (result != Result.Success || bytesWritten == 0 || peerId == null)
        {
            _bufferPool.Return(buffer);
            return false;
        }

        packet = new ReceivedPacket(buffer, (int)bytesWritten, peerId, channel == ServerChannel);
        return true;
    }

    private void ProcessPacket(ReceivedPacket packet)
    {
        try
        {
            ReadOnlySpan<byte> messageBuffer;

            if (FragmentHeader.IsFragment(packet.Buffer.AsSpan(0, packet.BytesWritten)))
            {
                if (!_fragmentReceiver.TryHandleFragment(
                    packet.Buffer,
                    packet.BytesWritten,
                    packet.PeerId.ToString(),
                    out byte[] reassembled))
                {
                    return; // Incomplete
                }
                messageBuffer = reassembled;
            }
            else
            {
                messageBuffer = new ReadOnlySpan<byte>(packet.Buffer, 0, packet.BytesWritten);
            }

            NativeMessageHandler.ReadMessage(new ReadableMessage
            {
                Buffer = messageBuffer,
                IsServerHandled = packet.IsServerHandled
            });
        }
        finally
        {
            _bufferPool.Return(packet.Buffer);
        }
    }

    private readonly struct ReceivedPacket
    {
        public readonly byte[] Buffer;
        public readonly int BytesWritten;
        public readonly ProductUserId PeerId;
        public readonly bool IsServerHandled;

        public ReceivedPacket(byte[] buffer, int bytesWritten, ProductUserId peerId, bool isServerHandled)
        {
            Buffer = buffer;
            BytesWritten = bytesWritten;
            PeerId = peerId;
            IsServerHandled = isServerHandled;
        }
    }
}