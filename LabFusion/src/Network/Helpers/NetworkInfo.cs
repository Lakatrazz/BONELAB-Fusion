namespace LabFusion.Network;

/// <summary>
/// Provides information about the status of the server.
/// </summary>
public static class NetworkInfo
{
    /// <summary>
    /// The active network transport layer. Points to <see cref="NetworkLayerManager.Layer"/>.
    /// </summary>
    public static NetworkLayer Layer => NetworkLayerManager.Layer;

    /// <summary>
    /// Returns the active network platform. If no layer is active, then it will return "None".
    /// </summary>
    public static string Platform => HasLayer ? Layer.Platform : "None";

    /// <summary>
    /// Returns if there is an active network layer.
    /// </summary>
    public static bool HasLayer => NetworkLayerManager.HasLayer;

    /// <summary>
    /// The active network lobby. Can be null. Allows you to read/write information from it.
    /// <para>Note that this will not write info for a lobby you have joined, but only a lobby you are hosting.</para>
    /// </summary>
    public static INetworkLobby Lobby => Layer.Lobby;

    /// <summary>
    /// Returns true if the user is currently in a server.
    /// </summary>
    public static bool HasServer => HasLayer && (Layer.IsHost || Layer.IsClient);

    /// <summary>
    /// Returns if the user is hosting the active server.
    /// </summary>
    public static bool IsHost => HasLayer && Layer.IsHost;

    /// <summary>
    /// Returns if the user is a client in the server and is not the host.
    /// </summary>
    public static bool IsClient => HasLayer && Layer.IsClient && !Layer.IsHost;

    /// <summary>
    /// Returns true if the networking solution allows the server to send messages to the host (Actual Server Logic vs P2P).
    /// </summary>
    public static bool ServerCanSendToHost => HasLayer && Layer.ServerCanSendToHost;

    /// <summary>
    /// The amount of bytes received this frame.
    /// </summary>
    public static int BytesDown { get; internal set; }

    /// <summary>
    /// The amount of bytes sent this frame.
    /// </summary>
    public static int BytesUp { get; internal set; }

    /// <summary>
    /// The userid from whoever sent the most recent message.
    /// </summary>
    public static ulong? LastReceivedUser { get; internal set; } = null;

    /// <summary>
    /// Checks if the message userId was spoofed based on the last received id. Only valid on the server's end.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static bool IsSpoofed(ulong userId)
    {
        // If the network layer cannot validate the user id, then we can't properly spoof check
        if (HasLayer && !Layer.RequiresValidId)
        {
            return false;
        }

        // If we haven't received any messages, then just assume it isn't spoofed
        if (!LastReceivedUser.HasValue)
        {
            return false;
        }

        return LastReceivedUser.Value != userId;
    }
}