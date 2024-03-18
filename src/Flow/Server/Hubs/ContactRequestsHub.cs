using Flow.Client.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Flow.Server.Hubs;

[Authorize]
public class ContactRequestsHub : Hub<IContactsClient>
{

}

public interface IContactsClient
{
    Task ReceiveContactRequestAsync(PendingRequestIncomingDto request);
    Task ReceiveSentContactRequestAsync(PendingRequestSentDto request);
    Task ReceiveNewContactAsync(ContactDto newContact);
    Task ReceiveCancelledRequestIdForRecipientAsync(Guid cancelledRequestId);
    Task ReceiveCancelledRequestIdForSenderAsync(Guid cancelledRequestId);
    Task ReceiveAcceptedRequestId(Guid acceptedRequestId);
    Task ReceiveNewContact(ContactDto newContact);
}
