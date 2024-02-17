using Flow.Shared.Enums;

namespace Flow.Shared.DataTransferObjects;

public class NotificationDto
{
    public Guid Id { get; set; }
    public NotificationType Type { get; set; }
    public string? Content { get; set; }
    public bool Seen { get; set; }
    public DateTime IssuedOn { get; set; }
    public string? RecipientId { get; set; }
}
