using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Flow.Shared.AuthRequests;

public class RegisterRequest
{
    [Required(ErrorMessage = "Your first name is required")]
    [MaxLength(250)]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Your last name is required")]
    [MaxLength(250)]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Your username is required")]
    [MaxLength(256)]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Your email is required")]
    [EmailAddress(ErrorMessage = "The email is not a valid email address")]
    public string? Email { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Your password is required")]
    [MinLength(6, ErrorMessage = "The password can't be shorter than 6 characters")]
    [MaxLength(25, ErrorMessage = "The password can't be longer than 25 characters")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Please repeat your password")]
    [Compare(nameof(Password), ErrorMessage = "The repeated password doesn't match your password")]
    public string? RepeatedPassword { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public PasswordValidationResult IsPasswordValid()
    {
        if (!string.IsNullOrEmpty(Password))
        {
            if (!Regex.IsMatch(Password, @"\d"))
            {
                return new PasswordValidationResult()
                {
                    ValidationMessage = "Password must contain at least one digit",
                    IsValid = false
                };
            }

            if (!Regex.IsMatch(Password, @"[a-z]"))
            {
                return new PasswordValidationResult()
                {
                    ValidationMessage = "Password must contain at least one letter",
                    IsValid = false
                };
            }

            if (!Regex.IsMatch(Password, @"[^a-zA-Z\d]"))
            {
                return new PasswordValidationResult()
                {
                    ValidationMessage = "Password must contain at least one non-alphanumeric character",
                    IsValid = false
                };
            }
        }

        return new PasswordValidationResult()
        {
            ValidationMessage = "Valid password",
            IsValid = true
        };
    }
}