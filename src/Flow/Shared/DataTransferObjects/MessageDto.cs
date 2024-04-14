using Flow.Shared.Enums;

namespace Flow.Shared.DataTransferObjects;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ThreadId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentOn { get; set; }
    public MessageStatus Status { get; set; }
    public MessageType Type { get; set; }
}
