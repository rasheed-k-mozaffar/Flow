using System.ComponentModel.DataAnnotations;
using Flow.Shared.Enums;

namespace Flow.Server.Models;

public class UserSettings
{
    [Key]
    public Guid Id { get; set; }

    public DateTime? EditedAt { get; set; } // * Auditory property

    [MaxLength(450)]
    public required string AppUserId { get; set; }

    public virtual AppUser? AppUser { get; set; }

    [MaxLength(150)]
    public string AccentsColor { get; set; } = "blue-600";

    [MaxLength(150)]
    public string SendButtonColor { get; set; } = "text-blue-500";

    [MaxLength(150)]
    public string MessageBubbleColor { get; set; } = "bg-blue-600";

    public bool EnableNotificationSounds { get; set; } = true;
    public bool EnableSentMessageSounds { get; set; } = true;

    public ActivityStatus ActivityStatus { get; set; }

    public Theme Theme { get; set; } = Theme.Light;
}
