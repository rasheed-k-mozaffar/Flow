namespace Flow.Shared.AuthRequests;

public class PasswordValidationResult
{
    public string? ValidationMessage { get; set; }
    public bool IsValid { get; set; }
}