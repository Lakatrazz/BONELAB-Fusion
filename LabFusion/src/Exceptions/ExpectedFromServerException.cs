namespace LabFusion.Exceptions;

public class ExpectedFromServerException : Exception
{
    public override string Message => "ExpectedFromServerException: A message was sent by a Client that can only be sent by the Server.";
}
