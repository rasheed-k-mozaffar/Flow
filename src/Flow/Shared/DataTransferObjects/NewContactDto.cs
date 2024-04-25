namespace Flow.Shared.DataTransferObjects;

public class NewChatDto
{
    public Guid ThreadId { get; set; }
    public List<UserDetailsDto> Participants { get; set; } = null!;
}
