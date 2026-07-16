namespace LabFusion.Exceptions;

/// <summary>
/// An exception thrown whenever the Server receives a Message that is only expected on a Client.
/// </summary>
public class MessageExpectedClientException : Exception
{
    public override string Message => "MessageExpectedClientException: Server has received a Message which expects a Client.";
}

