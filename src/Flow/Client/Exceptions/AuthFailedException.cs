namespace Flow.Client.Exceptions
{
    public class AuthFailedException : Exception
    {
        public List<string>? Errors { get; private set; }
        public AuthFailedException(string message, List<string>? errors = null)
                    : base(message)
        {
            Errors = errors;
        }
    }
}
