using System.ComponentModel.DataAnnotations;

namespace Flow.Shared.AuthRequests;

public class LoginRequest
{
    [Required(ErrorMessage = "Your email is required")]
    [EmailAddress(ErrorMessage = "The email is not a valid email address")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Your password is required")]
    [Range(6, 25, ErrorMessage = "The password must be between 6-25 characters")]
    public string? Password { get; set; }

    public bool IsPersistent { get; set; }
}