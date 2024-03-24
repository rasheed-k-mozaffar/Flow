namespace Flow.Shared.DataTransferObjects;

public class LoadPreviousMessagesRequest
{
    public Guid ThreadId { get; set; }
    public DateTime LastMessageDate { get; set; }
}

