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

    public DateTime? DateOfBirth { get; set; }

    public virtual Image? ProfilePicture { get; set; }

    public virtual UserSettings? Settings { get; set; }

    public virtual ICollection<ChatThread> Chats { get; set; } = new List<ChatThread>();
    public virtual ICollection<ContactRequest> ContactRequests { get; set; } = new List<ContactRequest>();
}
