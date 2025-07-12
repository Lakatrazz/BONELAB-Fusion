namespace LabFusion.Network;

internal static class EOSConnectionManager
{
    internal static void ConfigureP2P()
    {
        SetPortRange();
        ConfigureRelayControl();
    }

    internal static void Close()
    {
        var closeConnectionsOptions = new Epic.OnlineServices.P2P.CloseConnectionsOptions
        {
            LocalUserId = EOSNetworkLayer.LocalUserId,
            SocketId = EOSSocketHandler.SocketId
        };

        EOSManager.P2PInterface.CloseConnections(ref closeConnectionsOptions);
    }

    private static void SetPortRange()
    {
        var portOptions = new Epic.OnlineServices.P2P.SetPortRangeOptions()
        {
            Port = 7777,
            MaxAdditionalPortsToTry = 99,
        };
        EOSManager.P2PInterface.SetPortRange(ref portOptions);
    }

    private static void ConfigureRelayControl()
    {
        // Force relays to prevent IP leaking
        var relayOptions = new Epic.OnlineServices.P2P.SetRelayControlOptions()
        {
            RelayControl = Epic.OnlineServices.P2P.RelayControl.ForceRelays,
        };
        EOSManager.P2PInterface.SetRelayControl(ref relayOptions);
    }
}
