namespace LabFusion.Exceptions;

public class ExpectedOnClientException : Exception
{
    public override string Message => "ExpectedOnClientException: Server has received a Message which expects a Client.";
}

