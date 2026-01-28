using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

using LabFusion.Utilities;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Manages EOS P2P notification subscriptions. 
/// </summary>
internal class EOSNotificationManager
{
    private readonly ProductUserId _localUserId;
    private readonly SocketId _socketId;
    private readonly EOSConnectionHandler _connectionHandler;

    private ulong _connectionRequestedId = Common.INVALID_NOTIFICATIONID;
    private ulong _connectionEstablishedId = Common.INVALID_NOTIFICATIONID;
    private ulong _connectionClosedId = Common.INVALID_NOTIFICATIONID;

    public bool HasActiveNotifications =>
        _connectionRequestedId != Common.INVALID_NOTIFICATIONID ||
        _connectionEstablishedId != Common.INVALID_NOTIFICATIONID ||
        _connectionClosedId != Common.INVALID_NOTIFICATIONID;

    public EOSNotificationManager(
        ProductUserId localUserId,
        SocketId socketId,
        EOSConnectionHandler connectionHandler)
    {
        _localUserId = localUserId ?? throw new ArgumentNullException(nameof(localUserId));
        _socketId = socketId;
        _connectionHandler = connectionHandler ?? throw new ArgumentNullException(nameof(connectionHandler));
    }

    /// <summary>
    /// Registers notifications for hosting a server.
    /// </summary>
    public void RegisterHostNotifications()
    {
        if (EOSInterfaces.P2P == null)
        {
            FusionLogger.Error("Cannot register host notifications: P2P interface is null");
            return;
        }

        // Connection request notifications
        var requestOptions = new AddNotifyPeerConnectionRequestOptions
        {
            SocketId = _socketId,
            LocalUserId = _localUserId
        };

        _connectionRequestedId = EOSInterfaces.P2P.AddNotifyPeerConnectionRequest(
            ref requestOptions,
            null,
            _connectionHandler.OnConnectionRequested);

        // Connection closed notifications
        var closedOptions = new AddNotifyPeerConnectionClosedOptions
        {
            SocketId = _socketId,
            LocalUserId = _localUserId
        };

        _connectionClosedId = EOSInterfaces.P2P.AddNotifyPeerConnectionClosed(
            ref closedOptions,
            null,
            _connectionHandler.OnConnectionClosedAsHost);

#if DEBUG
        FusionLogger.Log("Registered host P2P notifications");
#endif
    }

    /// <summary>
    /// Registers notifications for joining a server as a client.
    /// </summary>
    public void RegisterClientNotifications()
    {
        if (EOSInterfaces.P2P == null)
        {
            FusionLogger.Error("Cannot register client notifications: P2P interface is null");
            return;
        }

        // Connection established notifications
        var establishedOptions = new AddNotifyPeerConnectionEstablishedOptions
        {
            SocketId = _socketId,
            LocalUserId = _localUserId
        };

        _connectionEstablishedId = EOSInterfaces.P2P.AddNotifyPeerConnectionEstablished(
            ref establishedOptions,
            null,
            _connectionHandler.OnConnectionEstablished);

        // Connection closed notifications
        var closedOptions = new AddNotifyPeerConnectionClosedOptions
        {
            SocketId = _socketId,
            LocalUserId = _localUserId
        };

        _connectionClosedId = EOSInterfaces.P2P.AddNotifyPeerConnectionClosed(
            ref closedOptions,
            null,
            _connectionHandler.OnConnectionClosedAsClient);

#if DEBUG
        FusionLogger.Log("Registered client P2P notifications");
#endif
    }

    /// <summary>
    /// Unregisters all active notifications.
    /// </summary>
    public void UnregisterAllNotifications()
    {
        if (EOSInterfaces.P2P == null)
            return;

        UnregisterNotification(ref _connectionRequestedId, EOSInterfaces.P2P.RemoveNotifyPeerConnectionRequest);
        UnregisterNotification(ref _connectionEstablishedId, EOSInterfaces.P2P.RemoveNotifyPeerConnectionEstablished);
        UnregisterNotification(ref _connectionClosedId, EOSInterfaces.P2P.RemoveNotifyPeerConnectionClosed);

#if DEBUG
        FusionLogger.Log("Unregistered all P2P notifications");
#endif
    }

    private static void UnregisterNotification(ref ulong notificationId, Action<ulong> removeAction)
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