namespace Flow.Shared.DataTransferObjects;

public class NewContactDto
{
    public Guid ThreadId { get; set; }
    public UserDetailsDto Contact { get; set; } = null!;
}
