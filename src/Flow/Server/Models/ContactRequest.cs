using Flow.Shared.Enums;

namespace Flow.Server.Models;

public class ContactRequest
{
    public Guid Id { get; set; }
    public string SenderId { get; set; } = null!;
    public string RecipientId { get; set; } = null!;
    public virtual AppUser Sender { get; set; } = null!;
    public virtual AppUser Recipient { get; set; } = null!;
    public RequestStatus Status { get; set; }
}
