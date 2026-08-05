namespace LabFusion.Network;

/// <summary>
/// Common routes when relaying a message from one client to another.
/// </summary>
public static class CommonMessageRoutes
{
    /// <summary>
    /// No relay is being used. The message is sent directly between server and client.
    /// </summary>
    public static readonly MessageRoute None = new(RelayType.None, NetworkChannel.Reliable);

    /// <summary>
    /// The message is guaranteed to arrive at the server.
    /// </summary>
    public static readonly MessageRoute ReliableToServer = new(RelayType.ToServer, NetworkChannel.Reliable);

    /// <summary>
    /// The message is guaranteed to arrive for every client.
    /// </summary>
    public static readonly MessageRoute ReliableToClients = new(RelayType.ToClients, NetworkChannel.Reliable);

    /// <summary>
    /// The message is sent to every client, but may be dropped.
    /// </summary>
    public static readonly MessageRoute UnreliableToClients = new(RelayType.ToClients, NetworkChannel.Unreliable);

    /// <summary>
    /// The message is guaranteed to arrive for every client except the sender.
    /// </summary>
    public static readonly MessageRoute ReliableToOtherClients = new(RelayType.ToOtherClients, NetworkChannel.Reliable);

    /// <summary>
    /// The message is sent to every client except the sender, but may be dropped.
    /// </summary>
    public static readonly MessageRoute UnreliableToOtherClients = new(RelayType.ToOtherClients, NetworkChannel.Unreliable);
}
