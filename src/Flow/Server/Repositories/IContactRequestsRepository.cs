using Flow.Shared.Enums;

namespace Flow.Server.Repositories;

public interface IContactRequestsRepository
{
    /// <summary>
    /// Adds a new contact request to the database, and a notification under the recipients id 
    /// to notify them about a pending contact request
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task SendConnectRequestAsync(string username, ContactRequest request);

    /// <summary>
    /// In case the request got accepted or declined, resolve the new status in the database
    /// If the new status is Accepted, a new chat thread is added to the database between the sender and recipient of
    /// the contact request.
    /// </summary>
    /// <returns></returns>
    Task ResolveConnectRequestAsync(Guid requestId, RequestStatus newStatus);

    /// <summary>
    /// Cancels a contact request from the sender's end, this would delete the record for it in the database
    /// </summary>
    /// <param name="requestId"></param>
    /// <returns></returns>
    Task CancelConnectRequestAsync(Guid requestId);

    /// <summary>
    /// Gets all the pending contact requests for the correspnding user type
    /// </summary>
    /// <param name="userType">
    ///     UserType.Sender: Gets the contact requests the user has made to other users (Pending)
    ///     UserType.Recipient: Gets the contact requests the user has received from other users
    /// </param>
    /// <returns></returns>
    Task<IEnumerable<ContactRequest>> GetPendingContactRequestsAsync(UserType userType);

    /// <summary>
    /// Gets the contacts for the user based on the ContactRequests table
    /// Any contact request the user has sent, or received, if accepted, it's a contact to be retrieved
    /// </summary>
    /// <returns></returns>
    Task<Dictionary<Guid, AppUser>> GetUserContactsAsync();

    /// <summary>
    /// Gets all the contact requests where the user is involved, requests Sent to or Sent from the 
    /// current authenticated user
    /// This will be used to perform search to determine which result is a contact, non contact, or a pending request
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<ContactRequest>> GetContactRequestsInvolvingUserAsync(CancellationToken cancellationToken);
}
