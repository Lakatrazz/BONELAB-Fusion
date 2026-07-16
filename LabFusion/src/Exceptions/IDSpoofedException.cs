namespace LabFusion.Exceptions;

/// <summary>
/// An exception thrown whenever a client attempts to spoof a SmallID or PlatformID.
/// </summary>
public class IDSpoofedException : Exception
{
    public override string Message => $"IDSpoofedException: Server received a spoofed ID from Client with PlatformID {PlatformID}.";

    public string PlatformID { get; set; } = null;

    public IDSpoofedException(string platformID)
    {
        PlatformID = platformID;
    }
}
