namespace LabFusion.Exceptions;

/// <summary>
/// An exception thrown whenever a Client receives a Message which is only expected on the Server.
/// </summary>
public class MessageExpectedServerException : Exception
{
    public override string Message => "MessageExpectedServerException: Client has received a Message which expects a Server.";
}
