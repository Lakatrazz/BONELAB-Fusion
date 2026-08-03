namespace LabFusion.Network;

/// <summary>
/// Manages general state for the network.
/// <para>For client specific state, see <see cref="ClientManager"/>.</para>
/// <para>For server specific state, see <see cref="ServerManager"/>.</para>
/// </summary>
public static class NetworkManager
{
    /// <summary>
    /// Returns true if a server exists, whether it is being ran or the client is connected to it.
    /// </summary>
    public static bool HasServer => ServerManager.IsServerRunning || ClientManager.IsClientConnected;

    /// <summary>
    /// If the client is connected to a server, this will return the ID of the server the client is connected to.
    /// <para>If a server is running on this instance, this will return the ID of the server being ran.</para>
    /// <para>Otherwise, <see cref="ServerID.Empty"/> will be returned.</para>
    /// </summary>
    public static ServerID ServerID =>
        ClientManager.IsClientConnected ? ClientManager.ConnectedServerID :
        ServerManager.IsServerRunning ? ServerManager.RunningServerID :
        ServerID.Empty;
}
