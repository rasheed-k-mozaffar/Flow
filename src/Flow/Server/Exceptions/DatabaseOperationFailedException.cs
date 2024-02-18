namespace Flow.Server.Exceptions;

public class DatabaseOperationFailedException : Exception
{
    public DatabaseOperationFailedException(string message) : base(message)
    {

    }
}
