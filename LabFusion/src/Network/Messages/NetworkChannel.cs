namespace LabFusion.Network;

/// <summary>
/// The channel that a message can be sent on, determining its priority.
/// </summary>
public enum NetworkChannel : byte
{
    /// <summary>
    /// Ensures that a message will be received. Use this for important messages such as one time events.
    /// </summary>
    Reliable,

    /// <summary>
    /// The message will be dropped due to network conditions and is not guaranteed to be received. 
    /// Use this for frequent messages such as a position update, or for messages that are not important such as visual effects.
    /// </summary>
    Unreliable,
}