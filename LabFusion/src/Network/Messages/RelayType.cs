namespace LabFusion.Network;

/// <summary>
/// The target that a message will be relayed to.
/// </summary>
public enum RelayType : byte
{
    /// <summary>
    /// No relay was used. The message was sent directly between the server and client.
    /// </summary>
    None,

    /// <summary>
    /// Relays the message to only the server.
    /// </summary>
    ToServer,

    /// <summary>
    /// Relays the message to all clients including the sender.
    /// </summary>
    ToClients,

    /// <summary>
    /// Relays the message to all other clients except for the sender.
    /// </summary>
    ToOtherClients,

    /// <summary>
    /// Relays the message to a set target user.
    /// </summary>
    ToTarget,

    /// <summary>
    /// Relays the message to multiple target users.
    /// </summary>
    ToTargets,
}