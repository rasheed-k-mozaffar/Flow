using System.ComponentModel.DataAnnotations;

namespace Flow.Shared.AuthRequests;

public class LoginRequest
{
    [Required(ErrorMessage = "Your email is required")]
    [EmailAddress(ErrorMessage = "The email is not a valid email address")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Your password is required")]
    [MinLength(6, ErrorMessage = "Your password must be at least 6 characters long")]
    [MaxLength(25, ErrorMessage = "Your password cannot be longer than 25 characters")]
    public string? Password { get; set; }

    public bool IsPersistent { get; set; }
}