using Steamworks;
using FusionHelper.Network;
using System.Runtime.InteropServices;
using LiteNetLib.Utils;

namespace FusionHelper.Steamworks
{
    public class SteamSocketManager
    {
        public Dictionary<ulong, HSteamNetConnection> ConnectedSteamIds = new();
        public HSteamListenSocket Socket;

        public HSteamNetPollGroup PollGroup;

        public void CreateRelay()
        {
            PollGroup = SteamNetworkingSockets.CreatePollGroup();
            Socket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, Array.Empty<SteamNetworkingConfigValue_t>());
        }

        public void KillRelay() {
            SteamNetworkingSockets.DestroyPollGroup(PollGroup);
            SteamNetworkingSockets.CloseListenSocket(Socket);
        }

        public void OnConnecting(SteamNetConnectionStatusChangedCallback_t info)
        {
            SteamNetworkingSockets.AcceptConnection(info.m_hConn);
        }

        public void OnConnected(SteamNetConnectionStatusChangedCallback_t info)
        {
            SteamNetworkingSockets.SetConnectionPollGroup(info.m_hConn, PollGroup);
        }

        public void OnDisconnected(SteamNetConnectionStatusChangedCallback_t info)
        {
            var pair = ConnectedSteamIds.FirstOrDefault((p) => p.Value.m_HSteamNetConnection == info.m_hConn.m_HSteamNetConnection);
            var longId = pair.Key;

            ConnectedSteamIds.Remove(longId);

            NetDataWriter writer = NetworkHandler.NewWriter(MessageTypes.OnDisconnected);
            writer.Put(longId);
            NetworkHandler.SendToClient(writer);
        }

        public void OnMessage(HSteamNetConnection connection, SteamNetworkingIdentity identity, IntPtr data, int size, long messageNum, long recvTime, int channel)
        {
            InsertConnection(identity.GetSteamID64(), connection);

            byte[] message = new byte[size];
            Marshal.Copy(data, message, 0, size);

            NetDataWriter writer = NetworkHandler.NewWriter(MessageTypes.OnMessage);
            writer.PutBytesWithLength(message);
            NetworkHandler.SendToClient(writer);
        }

        public void InsertConnection(ulong steamId, HSteamNetConnection connection) {
            ConnectedSteamIds[steamId] = connection;
        }

        public void Receive(int bufferSize = 32)
        {
            int processed = 0;
            IntPtr[] messageBuffer = new IntPtr[bufferSize];

            processed = SteamNetworkingSockets.ReceiveMessagesOnPollGroup(PollGroup, messageBuffer, bufferSize);

            for (int i = 0; i < processed; i++)
            {
                ReceiveMessage(Marshal.ReadIntPtr(messageBuffer, i * IntPtr.Size));
            }

            //
            // Overwhelmed our buffer, keep going
            //
            if (processed == bufferSize)
                Receive(bufferSize);
        }

        internal unsafe void ReceiveMessage(IntPtr msgPtr)
        {
            var msg = Marshal.PtrToStructure<NetMsg>(msgPtr);
            OnMessage(msg.Connection, msg.Identity, msg.DataPtr, msg.DataSize, msg.RecvTime, msg.MessageNumber, msg.Channel);
        }
    }

    // Yes, there is a Steamworks.NET enum for this, but I am not replacing all references to this to that mess.
    [Flags]
    public enum SendType : int
    {
        /// <summary>
        /// Send the message unreliably. Can be lost.  Messages *can* be larger than a
        /// single MTU (UDP packet), but there is no retransmission, so if any piece
        /// of the message is lost, the entire message will be dropped.
        ///
        /// The sending API does have some knowledge of the underlying connection, so
        /// if there is no NAT-traversal accomplished or there is a recognized adjustment
        /// happening on the connection, the packet will be batched until the connection
        /// is open again.
        /// </summary>
        Unreliable = 0,

        /// <summary>
        /// Disable Nagle's algorithm.
        /// By default, Nagle's algorithm is applied to all outbound messages.  This means
        /// that the message will NOT be sent immediately, in case further messages are
        /// sent soon after you send this, which can be grouped together.  Any time there
        /// is enough buffered data to fill a packet, the packets will be pushed out immediately,
        /// but partially-full packets not be sent until the Nagle timer expires. 
        /// </summary>
        NoNagle = 1 << 0,

        /// <summary>
        /// If the message cannot be sent very soon (because the connection is still doing some initial
        /// handshaking, route negotiations, etc), then just drop it.  This is only applicable for unreliable
        /// messages.  Using this flag on reliable messages is invalid.
        /// </summary>
        NoDelay = 1 << 2,

        /// Reliable message send. Can send up to 0.5mb in a single message. 
        /// Does fragmentation/re-assembly of messages under the hood, as well as a sliding window for
        /// efficient sends of large chunks of data.
        Reliable = 1 << 3
    }
}
