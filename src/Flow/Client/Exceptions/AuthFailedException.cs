namespace Flow.Client.Exceptions
{
    public class AuthFailedException : Exception
    {
        public AuthFailedException(string message)
                    : base(message) 
        {
            
        }
    }
}
