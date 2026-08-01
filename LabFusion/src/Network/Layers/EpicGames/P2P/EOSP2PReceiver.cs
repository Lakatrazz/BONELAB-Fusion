using System.Buffers;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using LabFusion.Utilities;

namespace LabFusion.Network;

internal class EOSP2PReceiver
{
    private const byte ServerChannel = 2;
    private const byte ClientChannel = 1;

    private const int MaxMessagesPerFrame = 512;

    internal EOSP2P P2P;
    private readonly FragmentAssembler _assembler;

    internal EOSP2PReceiver(EOSP2P p2p)
    {
        P2P = p2p;
        _assembler = new FragmentAssembler(P2P.BufferPool);
    }

    internal void Receive()
    {
        _assembler.CleanupIfNeeded();

        var getPacketSizeOptions = new GetNextReceivedPacketSizeOptions
        {
            LocalUserId = P2P.LocalUserId,
            RequestedChannel = null
        };

        var receiveOptions = new ReceivePacketOptions
        {
            LocalUserId = P2P.LocalUserId,
            RequestedChannel = null
        };

        for (int i = 0; i < MaxMessagesPerFrame; i++)
        {
            if (P2P.P2PInterface.GetNextReceivedPacketSize(ref getPacketSizeOptions, out uint packetSize) != Result.Success || packetSize == 0)
            {
                break;
            }
            
            byte[] buffer = P2P.BufferPool.Rent((int)packetSize);

            try
            {
                receiveOptions.MaxDataSizeBytes = packetSize;

                ProductUserId peerId = null;
                SocketId socketId = P2P.SocketId;

                var result = P2P.P2PInterface.ReceivePacket(ref receiveOptions, ref peerId, ref socketId, out byte channel, new ArraySegment<byte>(buffer, 0, (int)packetSize), out uint bytesWritten);
                if (result == Result.Success && bytesWritten > 0 && peerId != null && P2P.IsPeerConnected(peerId))
                {
                    ProcessPacket(buffer, (int)bytesWritten, peerId, channel == ServerChannel);
                    buffer = null;
                }
            }
            finally
            {
                if (buffer != null)
                    P2P.BufferPool.Return(buffer);
            }
        }
    }

    private void ProcessPacket(byte[] rawBuffer, int bytesWritten, ProductUserId peerId, bool isServerHandled)
    {
        try
        {
            ReadOnlySpan<byte> packetSpan = rawBuffer.AsSpan(0, bytesWritten);

            if (FragmentHeader.IsFragment(packetSpan))
            {
                if (_assembler.TryAssemble(peerId.ToString(), packetSpan, out byte[] completedBuffer, out int completedSize))
                {
                    try
                    {
                        NativeMessageHandler.ReadMessage(new ReadableMessage
                        {
                            Buffer = completedBuffer.AsSpan(0, completedSize),
                            IsServerHandled = isServerHandled
                        });
                    }
                    finally
                    {
                        P2P.BufferPool.Return(completedBuffer);
                    }
                }
            }
            else if (packetSpan.Length > FragmentHeader.KindPrefixSize)
            {
                NativeMessageHandler.ReadMessage(new ReadableMessage
                {
                    Buffer = packetSpan[FragmentHeader.KindPrefixSize..],
                    IsServerHandled = isServerHandled
                });
            }
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("processing EOS P2P packet", ex);
        }
        finally
        {
            P2P.BufferPool.Return(rawBuffer);
        }
    }
}