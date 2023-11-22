using Steamworks;
using FusionHelper.Network;
using System.Runtime.InteropServices;
using LiteNetLib.Utils;

namespace FusionHelper.Steamworks
{
    public class SteamConnectionManager
    {
        public HSteamNetConnection Connection;
        private Callback<SteamNetConnectionStatusChangedCallback_t> _onSteamNetConnectionUpdateCallback;

        public SteamConnectionManager()
        {
            _onSteamNetConnectionUpdateCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnStatusUpdate);
        }

        public void OnStatusUpdate(SteamNetConnectionStatusChangedCallback_t info)
        {
            // OnDisconnected
            if (info.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_None
                || info.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer
                || info.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally)
            {
                // If this is us, fully cleanup
                if (info.m_hConn == Connection)
                {
                    NetworkHandler.SendToClient(MessageTypes.OnConnectionDisconnected);

#if DEBUG
                    Console.WriteLine("Client was disconnected.");
#endif
                }

                SteamHandler.SocketManager?.OnDisconnected(info);
            }

            // OnConnecting
            if (info.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
            {
                // Is this correct?
                if (info.m_hConn != Connection)
                    SteamHandler.SocketManager?.OnConnecting(info);
            }

            // OnConnected
            if (info.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected)
            {
                // Is this correct?
                if (info.m_hConn != Connection)
                    SteamHandler.SocketManager?.OnConnected(info);
            }
        }

        public void Receive(int bufferSize = 32)
        {
            IntPtr[] messageBuffer = new IntPtr[bufferSize];

            int processed = SteamNetworkingSockets.ReceiveMessagesOnConnection(Connection, messageBuffer, bufferSize);

            int size = Marshal.SizeOf(typeof(IntPtr)) * messageBuffer.Length;
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(messageBuffer, 0, ptr, messageBuffer.Length);

            try
            {
                for (int i = 0; i < processed; i++)
                {
                    ReceiveMessage(Marshal.ReadIntPtr(ptr, i * IntPtr.Size));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            //
            // Overwhelmed our buffer, keep going
            //
            if (processed == bufferSize)
                Receive(bufferSize);
        }

        public void OnMessage(IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            byte[] message = new byte[size];
            Marshal.Copy(data, message, 0, size);

            NetDataWriter writer = NetworkHandler.NewWriter(MessageTypes.OnConnectionMessage);
            writer.PutBytesWithLength(message);
            NetworkHandler.SendToClient(writer);
        }

        internal unsafe void ReceiveMessage(IntPtr msgPtr)
        {
            var msg = Marshal.PtrToStructure<NetMsg>(msgPtr);
            OnMessage(msg.DataPtr, msg.DataSize, msg.RecvTime, msg.MessageNumber, msg.Channel);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal partial struct NetMsg
    {
        internal IntPtr DataPtr;
        internal int DataSize;
        internal HSteamNetConnection Connection;
        internal SteamNetworkingIdentity Identity;
        internal long ConnectionUserData;
        internal long RecvTime;
        internal long MessageNumber;
        internal IntPtr FreeDataPtr;
        internal IntPtr ReleasePtr;
        internal int Channel;
    }
}
