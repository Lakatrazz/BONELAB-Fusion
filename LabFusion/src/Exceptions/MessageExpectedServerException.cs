namespace LabFusion.Exceptions;

public class MessageExpectedServerException : Exception
{
    public override string Message => "MessageExpectedServerException: Client has received a Message which expects a Server.";
}
