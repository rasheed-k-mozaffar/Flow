using System.ComponentModel.DataAnnotations;
using Flow.Shared.Enums;

namespace Flow.Server.Models;

public class UserSettings
{
    [Key]
    public Guid Id { get; set; }
    public int ColorSchemeId { get; set; }

    [MaxLength(450)]
    public required string AppUserId { get; set; }
    public bool EnableNotificationSounds { get; set; } = true;
    public bool EnableSentMessageSounds { get; set; } = true;
    public ActivityStatus ActivityStatus { get; set; }
    public Theme Theme { get; set; } = Theme.Light;
    public DateTime? EditedAt { get; set; }

    public virtual AppUser? AppUser { get; set; }
    public virtual ColorScheme? ColorScheme { get; set; }
}
