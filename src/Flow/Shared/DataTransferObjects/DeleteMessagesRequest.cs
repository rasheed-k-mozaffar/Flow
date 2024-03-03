namespace Flow.Shared.DataTransferObjects;

public class DeleteMessagesRequest
{
    public Guid ThreadId { get; set; }
    public IEnumerable<Guid>? MessagesIds { get; set; }
}
