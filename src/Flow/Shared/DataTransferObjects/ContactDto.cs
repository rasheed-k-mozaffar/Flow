namespace Flow.Shared.DataTransferObjects;

public class ContactDto
{
    public Guid RequestId { get; set; }
    public Guid? ThreadId { get; set; }
    public UserDetailsDto? Contact { get; set; }
}
