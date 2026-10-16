using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using LabFusion.Network;

namespace MarrowFusion.Epic;

internal class EOSP2PClient
{
    internal EOSP2P P2P;

    internal bool IsConnecting;
    internal bool IsConnected;
    internal ProductUserId ConnectedUserId;
    
    private ulong ConnectionEstablishedId = Common.INVALID_NOTIFICATIONID;
    private ulong ConnectionClosedId = Common.INVALID_NOTIFICATIONID;
    
    internal EOSP2PClient(EOSP2P p2p)
    {
        P2P = p2p;
    }

    internal void Connect(ProductUserId remoteUserId)
    {
        if (!remoteUserId.IsValid())
            return;
        
        IsConnecting = true;
        IsConnected = false;
        ConnectedUserId = remoteUserId;

        SubscribeNotifications();
        
        // In order to make a connection you need to send a packet. Kinda dumb
        P2P.Sender.SendEmpty(remoteUserId);
    }

    internal void Disconnect()
    {
        if (ConnectedUserId == null || !ConnectedUserId.IsValid())
            return;
        
        var closeOptions = new CloseConnectionOptions
        {
            LocalUserId = P2P.LocalUserId,
            RemoteUserId = ConnectedUserId,
            SocketId = P2P.SocketId
        };

        P2P.P2PInterface.CloseConnection(ref closeOptions);
        
        IsConnecting = false;
        IsConnected = false;
        ConnectedUserId = null;
        
        UnsubscribeNotifications();
        
        P2P.Fragmenter.ClearAll();
    }

    private void SubscribeNotifications()
    {
        var establishedOptions = new AddNotifyPeerConnectionEstablishedOptions
        {
            LocalUserId = P2P.LocalUserId,
            SocketId = P2P.SocketId,
        };
        
        ConnectionEstablishedId = P2P.P2PInterface.AddNotifyPeerConnectionEstablished(ref establishedOptions, null, (ref OnPeerConnectionEstablishedInfo info) =>
        {
            if (info.RemoteUserId != ConnectedUserId)
                return;
            
            IsConnecting = false;
            IsConnected = true;
            
            EpicModule.Logger.Log($"Connected to server with ID: {info.RemoteUserId.ToString()}");
        });
        
        var closedOptions = new AddNotifyPeerConnectionClosedOptions
        {
            LocalUserId = P2P.LocalUserId,
            SocketId = P2P.SocketId,
        };
        
        ConnectionClosedId = P2P.P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null, (ref OnRemoteConnectionClosedInfo info) =>
        {
            if (info.RemoteUserId != ConnectedUserId)
                return;
            
            NetworkManager.DisconnectClientAndServer();
            
            EpicModule.Logger.Log($"Disconnected from server with ID: {info.RemoteUserId.ToString()}");
        });
    }

    private void UnsubscribeNotifications()
    {
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