using Flow.Shared.Enums;

namespace Flow.Shared.DataTransferObjects;

public class UserSettingsDto
{
    public Guid Id { get; set; }
    public Theme Theme { get; set; }
    public bool EnableNotificationSounds { get; set; }
    public bool EnableSentMessageSounds { get; set; }
    public ActivityStatus ActivityStatus { get; set; }
    public ColorSchemeDto? ColorScheme { get; set; }
    public int ColorSchemeId { get; set; }
}
