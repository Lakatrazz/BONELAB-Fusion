namespace LabFusion.Network;

/// <summary>
/// Necessary data that is passed into a MessageHandler so that it can be parsed before the message is read.
/// </summary>
public ref struct ReadableMessage
{
    /// <summary>
    /// The received bytes that will be read, parsed, and then handled in a MessageHandler.
    /// </summary>
    public ReadOnlySpan<byte> Buffer { get; set; }

    /// <summary>
    /// Whether or not this message is being handled on the server's end. Not always true for the host, as it could be handled on the host's client.
    /// </summary>
    public bool IsServerHandled { get; set; }
}
