namespace LabFusion.Exceptions;

public class MessageExpectedClientException : Exception
{
    public override string Message => "MessageExpectedClientException: Server has received a Message which expects a Client.";
}

