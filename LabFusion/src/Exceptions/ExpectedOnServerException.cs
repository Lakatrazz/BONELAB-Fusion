namespace LabFusion.Exceptions;

public class ExpectedOnServerException : Exception
{
    public override string Message => "ExpectedOnServerException: Client has received a Message which expects a Server.";
}
