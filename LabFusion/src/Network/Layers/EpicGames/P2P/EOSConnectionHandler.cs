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

    public EOSConnectionHandler(EOSP2PManager p2pManager, EOSConnectionStateManager connectionStateManager, Action onDisconnectedFromHost)
    {
        _p2pManager = p2pManager ?? throw new ArgumentNullException(nameof(p2pManager));
        _connectionStateManager = connectionStateManager ?? throw new ArgumentNullException(nameof(connectionStateManager));
        _onDisconnectedFromHost = onDisconnectedFromHost;
    }

    /// <summary>
    /// Called when a remote peer requests a connection (host only).
    /// </summary>
    public void OnConnectionRequested(ref OnIncomingConnectionRequestInfo info)
    {
        if (info.RemoteUserId == null)
            return;

        var result = _p2pManager.AcceptConnection(info.RemoteUserId);

#if DEBUG
        FusionLogger.Log($"Connection request from {info.RemoteUserId}:  {(result == Result.Success ? "Accepted" : $"Failed ({result})")}");
#endif
    }

    /// <summary>
    /// Called when a connection is established (client only).
    /// </summary>
    public void OnConnectionEstablished(ref OnPeerConnectionEstablishedInfo info)
    {
#if DEBUG
        FusionLogger.Log($"Connection established with {info.RemoteUserId}, Type: {info.ConnectionType}");
#endif

        ConnectionSender.SendConnectionRequest();
        
        _connectionStateManager.SetConnectionState(EOSConnectionStateManager.ConnectionState.Connected);
    }

    /// <summary>
    /// Called when a connection is closed (host handling).
    /// </summary>
    public void OnConnectionClosedAsHost(ref OnRemoteConnectionClosedInfo info)
    {
        if (info.RemoteUserId == null)
            return;

        var remoteId = info.RemoteUserId.ToString();

#if DEBUG
        FusionLogger.Log($"Connection closed with {remoteId}:  {info.Reason}");
#endif

        // Close our side of the connection
        _p2pManager.CloseConnection(info.RemoteUserId);

        // Handle player leaving if they were connected
        if (PlayerIDManager.HasPlayerID(remoteId))
        {
            InternalServerHelpers.OnPlayerLeft(remoteId);
            ConnectionSender.SendDisconnect(remoteId);
        }
    }

    /// <summary>
    /// Called when a connection is closed (client handling).
    /// </summary>
    public void OnConnectionClosedAsClient(ref OnRemoteConnectionClosedInfo info)
    {
#if DEBUG
        FusionLogger.Log($"Disconnected from host:  {info.Reason}");
#endif

        _onDisconnectedFromHost?.Invoke();
    }
}