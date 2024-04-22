namespace Flow.Shared.DataTransferObjects;

public class ChatDetails
{
    public Guid ChatThreadId { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
    public List<ContactDto> Participants { get; set; } = null!;
}
