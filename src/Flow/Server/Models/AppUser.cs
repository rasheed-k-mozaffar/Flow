using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Flow.Models;

public class AppUser : IdentityUser
{
    [MaxLength(250, ErrorMessage = "First name must be shorter than 250 characters")]
    public required string FirstName { get; set; }

    [MaxLength(250, ErrorMessage = "Last name must be shorter than 250 characters")]
    public required string LastName { get; set; }

    public string? ProfilePictureUrl { get; set; }
    public required DateOnly DateOfBirth { get; set; }
}
