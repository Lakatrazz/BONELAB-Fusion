namespace LabFusion.Network;

/// <summary>
/// Manages state and data transfer for the client connected to the server.
/// </summary>
public static class ClientManager
{
    /// <summary>
    /// Returns true if the client is actively connecting to a server, but hasn't connected yet.
    /// </summary>
    public static bool IsClientConnecting => false;

    /// <summary>
    /// Returns true if the client is actively connected to a server.
    /// </summary>
    public static bool IsClientConnected => NetworkInfo.IsClient;

    /// <summary>
    /// Returns true if the client is also hosting the server they are connected to.
    /// <para>If true, this means that a listen-server model is currently being used, rather than a separate dedicated server.</para>
    /// </summary>
    public static bool IsClientHost => IsClientConnected && ServerManager.IsServerRunning;

    /// <summary>
    /// Sends a message from the client to the connected server.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="channel"></param>
    public static void SendToServer(NetMessage message, NetworkChannel channel)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var layer = NetworkLayerManager.Layer;

        if (layer == null)
        {
            return;
        }

        NetworkInfo.BytesUp += message.Length;

        layer.ClientSendToServer(message, channel);
    }
}
