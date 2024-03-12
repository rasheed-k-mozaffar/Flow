using Flow.Shared.Enums;

namespace Flow.Shared.DataTransferObjects;

public class SendMessageDto
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public Guid ThreadId { get; set; }
    public string SenderId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public MessageType Type { get; set; }
}
