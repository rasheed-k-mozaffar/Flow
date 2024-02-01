using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Flow.Server.Models;

public class AppUser : IdentityUser
{
    [MaxLength(250)]
    public required string FirstName { get; set; }

    [MaxLength(250)]
    public required string LastName { get; set; }

    [MaxLength(2000)]
    public string? Bio { get; set; }

    public required DateOnly DateOfBirth { get; set; }

    public Image? ProfilePicture { get; set; }
}
