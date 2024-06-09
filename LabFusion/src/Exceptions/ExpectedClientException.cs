namespace LabFusion.Exceptions
{
    public class ExpectedClientException : Exception
    {
        public override string Message => "ExpectedClientException: Server has received a Message which expects a Client.";
    }
}

