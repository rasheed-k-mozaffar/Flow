namespace Flow.Server.Exceptions;

public class AuthFailedException : Exception
{
    public AuthFailedException(string message) : base(message)
    {

    }
}
