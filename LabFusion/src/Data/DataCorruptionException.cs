namespace LabFusion.Data;

public class DataCorruptionException : Exception
{
    public DataCorruptionException(string message) : base(message) { }

    public DataCorruptionException(string message, Exception inner) : base(message, inner) { }
}

