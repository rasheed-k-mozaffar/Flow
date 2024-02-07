namespace Flow.Shared.DataTransferObjects;

public class PendingRequestIncomingDto
{
    public Guid RequestId { get; set; }
    public UserDetailsDto? Sender { get; set; }
}
