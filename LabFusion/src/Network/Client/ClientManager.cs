using LabFusion.Senders;

namespace LabFusion.Network;

/// <summary>
/// Manages state and data transfer for the client connected to the server.
/// </summary>
public static class ClientManager
{
    /// <summary>
    /// Returns true if the client is actively connecting to a server and can send messages, but hasn't been accepted by the server yet.
    /// </summary>
    public static bool IsClientConnecting => IsLayerConnected && _attemptingConnection;

    /// <summary>
    /// Returns true if the client is actively connected to a server.
    /// </summary>
    public static bool IsClientConnected => IsLayerConnected && !_attemptingConnection;

    /// <summary>
    /// Returns true if the client is also hosting the server they are connected to.
    /// <para>If true, this means that a listen-server model is currently being used, rather than a separate dedicated server.</para>
    /// </summary>
    public static bool IsClientHost => NetworkLayerManager.Layer?.IsClientHost ?? false;

    /// <summary>
    /// Returns true if the client is connected to a server, but is not running the server.
    /// </summary>
    public static bool IsClientOnly => IsClientConnected && !IsClientHost;

    /// <summary>
    /// If the client is connected to a server, this will return the ID of the server the client is connected to.
    /// </summary>
    public static ServerID ConnectedServerID => NetworkLayerManager.Layer?.ConnectedServerID ?? ServerID.Empty;

    private static bool IsLayerConnected => NetworkLayerManager.Layer?.IsClientConnected ?? false;

    private static bool _attemptingConnection = false;

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

    internal static void OnConnectionEstablished()
    {
        _attemptingConnection = true;

        ConnectionSender.SendConnectionRequest();
    }

    internal static void OnConnectionLost()
    {
        _attemptingConnection = false;
    }
}
