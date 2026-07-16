namespace LabFusion.Network;

/// <summary>
/// The target that a message will be relayed to.
/// </summary>
public enum RelayType : byte
{
    /// <summary>
    /// Relays the message to the server, but without a proper "Sender" set. 
    /// Only use this before a proper ID has been established.
    /// <para>This relay type can only be used if the message has <see cref="MessageHandler.AllowDirectRelay"/> enabled OR when sent by the server.</para>
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