using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using LabFusion.Player;
using LabFusion.Senders;

namespace LabFusion.Network;

internal class EOSP2P : EOSInterface
{
    internal EOSRuntime Runtime;
    internal P2PInterface P2PInterface;
    internal ProductUserId LocalUserId;
    internal SocketId SocketId { get; } = new() { SocketName = "Fusion" };

    internal EOSBufferPool BufferPool;
    internal EOSP2PSender Sender;
    internal EOSP2PReceiver Receiver;
    internal HashSet<ProductUserId> ConnectedPeers = new HashSet<ProductUserId>(64);
    
    internal ulong ConnectionRequestedId = Common.INVALID_NOTIFICATIONID;
    internal ulong ConnectionEstablishedId = Common.INVALID_NOTIFICATIONID;
    internal ulong ConnectionClosedId = Common.INVALID_NOTIFICATIONID;
    
    internal EOSP2P(EOSRuntime eosRuntime, P2PInterface p2pInterface, ProductUserId localUserId)
    {
        Runtime = eosRuntime;
        P2PInterface = p2pInterface;
        LocalUserId = localUserId;
        
        BufferPool = new EOSBufferPool();
        Sender = new EOSP2PSender(this);
        Receiver = new EOSP2PReceiver(this);
    }
    
    internal override IEnumerator InitializeAsync(Action<bool> onComplete)
    {
        var setPortRangeOptions = new SetPortRangeOptions { Port = 7777, MaxAdditionalPortsToTry = 99 };
        P2PInterface.SetPortRange(ref setPortRangeOptions);
        var setRelayControlOptions = new SetRelayControlOptions { RelayControl = RelayControl.ForceRelays };
        P2PInterface.SetRelayControl(ref setRelayControlOptions);

        onComplete?.Invoke(true);
        
        yield return null;
    }

    internal void Connect(ProductUserId remoteUserId, Action onConnected)
    {
        RemoveAllPeerNotifications();
        
        var establishedOptions = new AddNotifyPeerConnectionEstablishedOptions
        {
            SocketId = SocketId,
            LocalUserId = LocalUserId
        };
        
        ConnectionEstablishedId = P2PInterface.AddNotifyPeerConnectionEstablished(ref establishedOptions, null, (ref OnPeerConnectionEstablishedInfo info) =>
        {
            onConnected?.Invoke();
        });
        
        var closedOptions = new AddNotifyPeerConnectionClosedOptions
        {
            SocketId = SocketId,
            LocalUserId = LocalUserId
        };
        
        ConnectionClosedId = P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null, (ref OnRemoteConnectionClosedInfo info) =>
        {
            NetworkHelper.Disconnect();
        });
        
        // EOS is weird
        // Send a dummy packet to establish a connection
        Sender.Send(remoteUserId, new byte[] { 0 }, NetworkChannel.Reliable, false);
    }

    // For the host to register connection notifications and add themselves to ConnectedPeers
    internal void ConnectSelf()
    {
        RemoveAllPeerNotifications();
        
        var requestOptions = new AddNotifyPeerConnectionRequestOptions()
        {
            SocketId = SocketId,
            LocalUserId = LocalUserId
        };
        
        ConnectionRequestedId = P2PInterface.AddNotifyPeerConnectionRequest(ref requestOptions, null, (ref OnIncomingConnectionRequestInfo  info) =>
        {
            var options = new AcceptConnectionOptions { LocalUserId = LocalUserId, RemoteUserId = info.RemoteUserId, SocketId = SocketId };
            P2PInterface.AcceptConnection(ref options);
        });
        
        var establishedOptions = new AddNotifyPeerConnectionEstablishedOptions
        {
            SocketId = SocketId,
            LocalUserId = LocalUserId
        };
        
        ConnectionEstablishedId = P2PInterface.AddNotifyPeerConnectionEstablished(ref establishedOptions, null, (ref OnPeerConnectionEstablishedInfo info) =>
        {
            ConnectedPeers.Add(info.RemoteUserId);
        });
        
        var closedOptions = new AddNotifyPeerConnectionClosedOptions
        {
            SocketId = SocketId,
            LocalUserId = LocalUserId
        };
        
        ConnectionClosedId = P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null, (ref OnRemoteConnectionClosedInfo info) =>
        {
            ConnectedPeers.Remove(info.RemoteUserId);
            
            var remoteUserId = info.RemoteUserId.ToString();
            if (PlayerIDManager.HasPlayerID(remoteUserId))
            {
                InternalServerHelpers.OnPlayerLeft(remoteUserId);
                ConnectionSender.SendDisconnect(remoteUserId);
            }
        });
        
        ConnectedPeers.Add(LocalUserId);
    }

    // Can be called as host or client. Kills all p2p connections
    internal void Disconnect()
    {
        RemoveAllPeerNotifications();
        var closeConnectionsOptions = new CloseConnectionsOptions { LocalUserId = LocalUserId, SocketId = SocketId };
        P2PInterface.CloseConnections(ref closeConnectionsOptions);
        ConnectedPeers.Clear();
    }
    
    // Can only be run as host. Kills the connection to a specific user.
    internal void DisconnectUser(ProductUserId remoteUserId)
    {
        var closeConnectionOptions = new CloseConnectionOptions { LocalUserId = LocalUserId, RemoteUserId = remoteUserId, SocketId = SocketId };
        P2PInterface.CloseConnection(ref closeConnectionOptions);
    }

    internal void RemoveAllPeerNotifications()
    {
        if (ConnectionRequestedId != Common.INVALID_NOTIFICATIONID)
        {
            P2PInterface.RemoveNotifyPeerConnectionRequest(ConnectionRequestedId);
            ConnectionRequestedId = Common.INVALID_NOTIFICATIONID;
        }
        
        if (ConnectionEstablishedId != Common.INVALID_NOTIFICATIONID)
        {
            P2PInterface.RemoveNotifyPeerConnectionEstablished(ConnectionEstablishedId);
            ConnectionEstablishedId = Common.INVALID_NOTIFICATIONID;
        }
        
        if (ConnectionClosedId != Common.INVALID_NOTIFICATIONID)
        {
            P2PInterface.RemoveNotifyPeerConnectionClosed(ConnectionClosedId);
            ConnectionClosedId = Common.INVALID_NOTIFICATIONID;
        }
    }
}