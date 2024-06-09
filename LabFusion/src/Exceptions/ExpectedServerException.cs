namespace LabFusion.Exceptions
{
    public class ExpectedServerException : Exception
    {
        public override string Message => "ExpectedServerException: Client has received a Message which expects a Server.";
    }
}
