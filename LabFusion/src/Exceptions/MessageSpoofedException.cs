namespace LabFusion.Exceptions;

/// <summary>
/// An exception thrown whenever the Server receives a message spoofed by the Client who sent it.
/// </summary>
public class MessageSpoofedException : Exception
{
    public override string Message => $"MessageSpoofedException: Server received a spoofed message from Client with PlatformID {PlatformID}.";

    public string PlatformID { get; set; } = null;

    public MessageSpoofedException(string platformID)
    {
        PlatformID = platformID;
    }
}
