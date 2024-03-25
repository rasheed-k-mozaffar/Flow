namespace Flow.Shared.DataTransferObjects;

public class ChatDetails
{
    public Guid ChatThreadId { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
    public UserDetailsDto Contact { get; set; } = null!;
}
