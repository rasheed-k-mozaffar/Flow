namespace Flow.Server.Models;

public class UserManagerResponse
{
    public required string Message { get; set; }
    public string? Token { get; set; }
    public bool Succeeded { get; set; }
    public bool IsLockedOut { get; set; }
}
