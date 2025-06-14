namespace LabFusion.Network;

public enum RelayType : byte
{
    /// <summary>
    /// Relays the message to the server, but without a proper "Sender" set. Only use this before a proper ID has been established.
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