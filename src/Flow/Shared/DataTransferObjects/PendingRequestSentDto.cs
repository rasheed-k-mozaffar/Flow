namespace Flow.Shared.DataTransferObjects;

public class PendingRequestSentDto
{
    public Guid RequestId { get; set; }
    public UserDetailsDto? Recipient { get; set; }
}
