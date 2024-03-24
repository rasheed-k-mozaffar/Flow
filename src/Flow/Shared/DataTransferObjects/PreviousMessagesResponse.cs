namespace Flow.Shared.DataTransferObjects;

public class PreviousMessagesResponse
{
    public List<MessageDto> Messages { get; set; }
    public bool HasUnloadedMessages { get; set; }
    public int UnloadedMessageCount { get; set; }

    public PreviousMessagesResponse()
    {
        Messages = new();
    }
}
