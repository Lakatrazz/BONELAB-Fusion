namespace LabFusion.Exceptions;

public class ExpectedFromClientException : Exception
{
    public override string Message => "ExpectedFromClientException: A message was sent by the Server that can only be sent by a Client.";
}
