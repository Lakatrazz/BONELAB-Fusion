using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using LabFusion.Network;

namespace MarrowFusion.Epic;

internal class EOSP2PServer
{
    internal EOSP2P P2P;

    internal bool IsRunning;
    internal HashSet<ProductUserId> ConnectedPeers = new();
    
    private ulong ConnectionRequestedId = Common.INVALID_NOTIFICATIONID;
    private ulong ConnectionEstablishedId = Common.INVALID_NOTIFICATIONID;
    private ulong ConnectionClosedId = Common.INVALID_NOTIFICATIONID;
    
    internal EOSP2PServer(EOSP2P p2p)
    {
        P2P = p2p;
    }

    internal void Start()
    {
        IsRunning = true;
        
        SubscribeNotifications();
    }

    internal void Stop()
    {
        IsRunning = false;
        DisconnectAll();
        ConnectedPeers?.Clear();
        
        UnsubscribeNotifications();
    }

    internal void DisconnectAll()
    {
        foreach (var connectedPeerId in ConnectedPeers)
        {
            DisconnectPeer(connectedPeerId);
        }
    }
    
    internal void DisconnectPeer(ProductUserId remoteUserId)
    {
        if (!remoteUserId.IsValid())
            return;
        
        var closeOptions = new CloseConnectionOptions
        {
            LocalUserId = P2P.LocalUserId, 
            RemoteUserId = remoteUserId, 
            SocketId = P2P.SocketId
        };
        
        P2P.P2PInterface.CloseConnection(ref closeOptions);
    }

    private void SubscribeNotifications()
    {
        var requestOptions = new AddNotifyPeerConnectionRequestOptions()
        {
            LocalUserId = P2P.LocalUserId,
            SocketId = P2P.SocketId,
        };
        
        ConnectionRequestedId = P2P.P2PInterface.AddNotifyPeerConnectionRequest(ref requestOptions, null, (ref OnIncomingConnectionRequestInfo  info) =>
        {
            var acceptOptions = new AcceptConnectionOptions
            {
                LocalUserId = P2P.LocalUserId, 
                RemoteUserId = info.RemoteUserId, 
                SocketId = P2P.SocketId,
            };
            
            P2P.P2PInterface.AcceptConnection(ref acceptOptions);
        });
        
        var establishedOptions = new AddNotifyPeerConnectionEstablishedOptions
        {
            LocalUserId = P2P.LocalUserId,
            SocketId = P2P.SocketId,
        };
        
        ConnectionEstablishedId = P2P.P2PInterface.AddNotifyPeerConnectionEstablished(ref establishedOptions, null, (ref OnPeerConnectionEstablishedInfo info) =>
        {
            ConnectedPeers.Add(info.RemoteUserId);

            EpicModule.Logger.Log($"Client connected with ID: {info.RemoteUserId.ToString()}");
        });
        
        var closedOptions = new AddNotifyPeerConnectionClosedOptions
        {
            LocalUserId = P2P.LocalUserId,
            SocketId = P2P.SocketId,
        };
        
        ConnectionClosedId = P2P.P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null, (ref OnRemoteConnectionClosedInfo info) =>
        {
            ConnectedPeers.Remove(info.RemoteUserId);
            
            var platformID = new ClientPlatformID(info.RemoteUserId.ToString());
            
            NetworkLayerManager.Layer?.DisconnectClient(platformID);
            
            P2P.Fragmenter.ClearPendingForSender(info.RemoteUserId);
            
            EpicModule.Logger.Log($"Client disconnected with ID: {info.RemoteUserId.ToString()}");
        });
    }

    private void UnsubscribeNotifications()
    {
        if (ConnectionRequestedId != Common.INVALID_NOTIFICATIONID)
        {
            P2P.P2PInterface.RemoveNotifyPeerConnectionRequest(ConnectionRequestedId);
            ConnectionRequestedId = Common.INVALID_NOTIFICATIONID;
        }

        if (ConnectionEstablishedId != Common.INVALID_NOTIFICATIONID)
        {
            P2P.P2PInterface.RemoveNotifyPeerConnectionEstablished(ConnectionEstablishedId);
            ConnectionEstablishedId = Common.INVALID_NOTIFICATIONID;
        }

        if (ConnectionClosedId != Common.INVALID_NOTIFICATIONID)
        {
            P2P.P2PInterface.RemoveNotifyPeerConnectionClosed(ConnectionClosedId);
            ConnectionClosedId = Common.INVALID_NOTIFICATIONID;
        }
    }
}