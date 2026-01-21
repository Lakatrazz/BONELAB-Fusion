using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

namespace LabFusion.Network.EpicGames;

/// <summary>
/// Manages P2P configuration and connection lifecycle.
/// </summary>
internal class EOSP2PManager
{
    private const ushort BasePort = 7777;
    private const ushort MaxAdditionalPorts = 99;

    private readonly ProductUserId _localUserId;
    private readonly SocketId _socketId;

    public EOSP2PManager(ProductUserId localUserId, SocketId socketId)
    {
        _localUserId = localUserId ?? throw new ArgumentNullException(nameof(localUserId));
        _socketId = socketId;
    }

    public void Configure()
    {
        if (EOSInterfaces.P2P == null)
            return;

        ConfigurePortRange();
        ConfigureRelayControl();
    }

    public Result AcceptConnection(ProductUserId remoteUserId)
    {
        if (EOSInterfaces.P2P == null || remoteUserId == null)
            return Result.InvalidState;

        var options = new AcceptConnectionOptions
        {
            RemoteUserId = remoteUserId,
            SocketId = _socketId,
            LocalUserId = _localUserId
        };

        return EOSInterfaces.P2P.AcceptConnection(ref options);
    }

    public Result CloseConnection(ProductUserId remoteUserId)
    {
        if (EOSInterfaces.P2P == null || remoteUserId == null)
            return Result.InvalidState;

        var options = new CloseConnectionOptions
        {
            RemoteUserId = remoteUserId,
            SocketId = _socketId,
            LocalUserId = _localUserId
        };

        return EOSInterfaces.P2P.CloseConnection(ref options);
    }

    public Result CloseAllConnections()
    {
        if (EOSInterfaces.P2P == null)
            return Result.InvalidState;

        var options = new CloseConnectionsOptions
        {
            LocalUserId = _localUserId,
            SocketId = _socketId
        };

        return EOSInterfaces.P2P.CloseConnections(ref options);
    }

    private void ConfigurePortRange()
    {
        var options = new SetPortRangeOptions
        {
            Port = BasePort,
            MaxAdditionalPortsToTry = MaxAdditionalPorts
        };

        EOSInterfaces.P2P.SetPortRange(ref options);
    }

    private void ConfigureRelayControl()
    {
        var options = new SetRelayControlOptions
        {
            RelayControl = RelayControl.ForceRelays
        };

        EOSInterfaces.P2P.SetRelayControl(ref options);
    }
}