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
}
