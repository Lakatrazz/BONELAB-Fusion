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
    /// The amount of bytes received this frame.
    /// </summary>
    public static int BytesDown { get; internal set; }

    /// <summary>
    /// The amount of bytes sent this frame.
    /// </summary>
    public static int BytesUp { get; internal set; }
}