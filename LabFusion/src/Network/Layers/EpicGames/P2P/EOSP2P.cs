using System.Collections;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;

namespace LabFusion.Network;

internal class EOSP2P : EOSInterface
{
    internal EOSRuntime Runtime;
    internal P2PInterface P2PInterface;
    internal ProductUserId LocalUserId;
    internal SocketId SocketId { get; } = new() { SocketName = "Fusion" };
    internal Action<ProductUserId> OnConnected;

    internal EOSP2PSender Sender;
    internal EOSP2PReceiver Receiver;
    internal HashSet<ProductUserId> ConnectedPeers = new HashSet<ProductUserId>(64);
    internal HashSet<ProductUserId> PendingPeers = new HashSet<ProductUserId>(64);
    
    internal ulong ConnectionRequestedId = Common.INVALID_NOTIFICATIONID;
    internal ulong ConnectionEstablishedId = Common.INVALID_NOTIFICATIONID;
    internal ulong ConnectionClosedId = Common.INVALID_NOTIFICATIONID;
    
    internal EOSP2P(EOSRuntime eosRuntime, P2PInterface p2pInterface, ProductUserId localUserId)
    {
        Runtime = eosRuntime;
        P2PInterface = p2pInterface;
        LocalUserId = localUserId;
        
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

    internal void Connect(ProductUserId remoteUserId)
    {
        if (IsPeerConnected(remoteUserId))
        {
            FusionLogger.Log($"Already connected to {remoteUserId}");
            return;
        }

        if (!AddPendingPeer(remoteUserId))
        {
            FusionLogger.Log($"Connection attempt already in progress for {remoteUserId}");
            return;
        }

        // EOS is weird
        // Send a dummy packet to establish a connection
        Sender.Send(remoteUserId, new byte[] { 0 }, NetworkChannel.Reliable, false);
    }

    internal void ConnectUser(ProductUserId remoteUserId)
    {
        if (IsPeerConnected(remoteUserId))
        {
            FusionLogger.Log($"Already connected to {remoteUserId}");
            return;
        }

        if (!AddPendingPeer(remoteUserId))
        {
            FusionLogger.Log($"Connection attempt already in progress for {remoteUserId}");
            return;
        }
        
        var options = new AcceptConnectionOptions { LocalUserId = LocalUserId, RemoteUserId = remoteUserId, SocketId = SocketId };
        P2PInterface.AcceptConnection(ref options);
    }

    internal void Disconnect()
    {
        ClearConnectedPeers();
        var closeConnectionsOptions = new CloseConnectionsOptions { LocalUserId = LocalUserId, SocketId = SocketId };
        P2PInterface.CloseConnections(ref closeConnectionsOptions);
    }
    
    internal void DisconnectUser(ProductUserId remoteUserId)
    {
        var closeConnectionOptions = new CloseConnectionOptions { LocalUserId = LocalUserId, RemoteUserId = remoteUserId, SocketId = SocketId };
        P2PInterface.CloseConnection(ref closeConnectionOptions);
    }
    
    internal bool AddConnectedPeer(ProductUserId remoteUserId) => ConnectedPeers.Add(remoteUserId);
    
    internal bool RemoveConnectedPeer(ProductUserId remoteUserId) => ConnectedPeers.Remove(remoteUserId);
    
    internal bool AddPendingPeer(ProductUserId remoteUserId) => PendingPeers.Add(remoteUserId);
    
    internal bool RemovePendingPeer(ProductUserId remoteUserId) => PendingPeers.Remove(remoteUserId);
    
    internal void ClearConnectedPeers()
    {
        ConnectedPeers.Clear();
        PendingPeers.Clear();
    }
    
    internal bool IsPeerConnected(ProductUserId remoteUserId) => ConnectedPeers.Contains(remoteUserId);
    
    internal void OnConnectionRequestedAsHost(ref OnIncomingConnectionRequestInfo info)
    {
        ConnectUser(info.RemoteUserId);
    }
    
    internal void OnConnectionEstablishedAsClient(ref OnPeerConnectionEstablishedInfo info)
    {
        RemovePendingPeer(info.RemoteUserId);
        AddConnectedPeer(info.RemoteUserId);
        OnConnected?.Invoke(info.RemoteUserId);
    }
    
    internal void OnConnectionEstablishedAsHost(ref OnPeerConnectionEstablishedInfo info)
    {
        RemovePendingPeer(info.RemoteUserId);
        AddConnectedPeer(info.RemoteUserId);
        OnConnected?.Invoke(info.RemoteUserId);
    }
    
    internal void OnConnectionClosedAsClient(ref OnRemoteConnectionClosedInfo info)
    {
        RemovePendingPeer(info.RemoteUserId);
        RemoveConnectedPeer(info.RemoteUserId);
        NetworkHelper.Disconnect();
    }
    
    internal void OnConnectionClosedAsHost(ref OnRemoteConnectionClosedInfo info)
    {
        RemovePendingPeer(info.RemoteUserId);
        RemoveConnectedPeer(info.RemoteUserId);
        DisconnectUser(info.RemoteUserId);

        var remoteId = info.RemoteUserId.ToString();
        if (PlayerIDManager.HasPlayerID(remoteId))
        {
            InternalServerHelpers.OnPlayerLeft(remoteId);
            ConnectionSender.SendDisconnect(remoteId);
        }
    }
    
    internal void RegisterHostNotifications()
    {
        UnregisterAllNotifications();
        
        var requestOptions = new AddNotifyPeerConnectionRequestOptions
        {
            SocketId = SocketId,
            LocalUserId = LocalUserId
        };

        ConnectionRequestedId = P2PInterface.AddNotifyPeerConnectionRequest(ref requestOptions, null, OnConnectionRequestedAsHost);
        
        var establishedOptions = new AddNotifyPeerConnectionEstablishedOptions
        {
            SocketId = SocketId,
            LocalUserId = LocalUserId
        };

        ConnectionEstablishedId = P2PInterface.AddNotifyPeerConnectionEstablished(ref establishedOptions, null, OnConnectionEstablishedAsHost);
        
        var closedOptions = new AddNotifyPeerConnectionClosedOptions
        {
            SocketId = SocketId,
            LocalUserId = LocalUserId
        };

        ConnectionClosedId = P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null, OnConnectionClosedAsHost);
    }
    
    internal void RegisterClientNotifications()
    {
        UnregisterAllNotifications();
        
        var establishedOptions = new AddNotifyPeerConnectionEstablishedOptions
        {
            SocketId = SocketId,
            LocalUserId = LocalUserId
        };

        ConnectionEstablishedId = P2PInterface.AddNotifyPeerConnectionEstablished(ref establishedOptions, null, OnConnectionEstablishedAsClient);
        
        var closedOptions = new AddNotifyPeerConnectionClosedOptions
        {
            SocketId = SocketId,
            LocalUserId = LocalUserId
        };

        ConnectionClosedId = P2PInterface.AddNotifyPeerConnectionClosed(ref closedOptions, null, OnConnectionClosedAsClient);
    }
    
    internal void UnregisterAllNotifications()
    {
        UnregisterNotification(ref ConnectionRequestedId, P2PInterface.RemoveNotifyPeerConnectionRequest);
        UnregisterNotification(ref ConnectionEstablishedId, P2PInterface.RemoveNotifyPeerConnectionEstablished);
        UnregisterNotification(ref ConnectionClosedId, P2PInterface.RemoveNotifyPeerConnectionClosed);
    }

    private void UnregisterNotification(ref ulong notificationId, Action<ulong> removeAction)
    {
        if (notificationId == Common.INVALID_NOTIFICATIONID)
            return;

        try
        {
            removeAction(notificationId);
        }
        catch (Exception ex)
        {
            FusionLogger.LogException("removing P2P notification", ex);
        }
        finally
        {
            notificationId = Common.INVALID_NOTIFICATIONID;
        }
    }
}