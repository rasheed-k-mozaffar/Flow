using Flow.Shared.Enums;

namespace Flow.Shared.DataTransferObjects;

public class MessageDto
{
    public required Guid Id { get; set; }
    public required Guid ThreadId { get; set; }
    public required string SenderId { get; set; }
    public required string Content { get; set; }
    public DateTime SentOn { get; set; }
    public MessageStatus Status { get; set; }
}
