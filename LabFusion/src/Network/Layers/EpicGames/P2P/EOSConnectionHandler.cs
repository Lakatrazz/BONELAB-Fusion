using Epic.OnlineServices;
using Epic.OnlineServices.P2P;
using LabFusion.Player;
using LabFusion.Senders;
using LabFusion.Utilities;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Handles P2P connection events and callbacks.
/// </summary>
internal class EOSConnectionHandler
{
    private readonly EOSP2PManager _p2pManager;
    private readonly EOSConnectionStateManager _connectionStateManager;
    private readonly Action _onDisconnectedFromHost;

    internal EOSConnectionHandler(EOSP2PManager p2pManager, EOSConnectionStateManager connectionStateManager, Action onDisconnectedFromHost)
    {
        _p2pManager = p2pManager ?? throw new ArgumentNullException(nameof(p2pManager));
        _connectionStateManager = connectionStateManager ?? throw new ArgumentNullException(nameof(connectionStateManager));
        _onDisconnectedFromHost = onDisconnectedFromHost;
    }
    
    internal void OnConnectionRequestedAsHost(ref OnIncomingConnectionRequestInfo info)
    {
        if (info.RemoteUserId == null)
            return;

        var result = _p2pManager.AcceptConnection(info.RemoteUserId);
    }
    
    internal void OnConnectionEstablishedAsClient(ref OnPeerConnectionEstablishedInfo info)
    {
        ConnectionSender.SendConnectionRequest();
        
        _connectionStateManager.SetConnectionState(EOSConnectionStateManager.ConnectionState.Connected);
    }
    
    internal void OnConnectionEstablishedAsHost(ref OnPeerConnectionEstablishedInfo info)
    {
        if (info.RemoteUserId == null)
            return;

        EOSMessenger.AddConnectedClient(info.RemoteUserId);
    }
    
    internal void OnConnectionClosedAsClient(ref OnRemoteConnectionClosedInfo info)
    {
        _onDisconnectedFromHost?.Invoke();
    }
    
    internal void OnConnectionClosedAsHost(ref OnRemoteConnectionClosedInfo info)
    {
        if (info.RemoteUserId == null)
            return;

        var remoteId = info.RemoteUserId.ToString();

        EOSMessenger.RemoveConnectedClient(info.RemoteUserId);

        // Close our side of the connection
        _p2pManager.CloseConnection(info.RemoteUserId);

        // Handle player leaving if they were connected
        if (PlayerIDManager.HasPlayerID(remoteId))
        {
            InternalServerHelpers.OnPlayerLeft(remoteId);
            ConnectionSender.SendDisconnect(remoteId);
        }
    }
}