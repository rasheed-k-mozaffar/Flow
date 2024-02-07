using System.ComponentModel.DataAnnotations;
using Flow.Shared.Enums;

namespace Flow.Server.Models;

public class Notification
{
    [Key]
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string? Content { get; set; }
    public DateTime IssuedOn { get; set; }

    public virtual AppUser Recipient { get; set; } = null!;
    public string RecipientId { get; set; } = null!;
    public bool Seen { get; set; }
}
