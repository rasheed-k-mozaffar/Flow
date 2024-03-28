using Flow.Server.Hubs;
using Flow.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Flow.Server.Repositories;

public class ContactRequestsRepository : IContactRequestsRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<ContactRequestsRepository> _logger;
    private readonly IHubContext<ContactRequestsHub, IContactsClient> _contactsHubContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly UserInfo _userInfo;

    public ContactRequestsRepository
    (
        AppDbContext db,
        ILogger<ContactRequestsRepository> logger,
        UserInfo userInfo,
        IHubContext<ContactRequestsHub, IContactsClient> contactsHubContext,
        UserManager<AppUser> userManager
    )
    {
        _db = db;
        _logger = logger;
        _userInfo = userInfo;
        _contactsHubContext = contactsHubContext;
        _userManager = userManager;
    }


    public async Task SendConnectRequestAsync(string username, ContactRequest request)
    {
        // find the recipient
        var recipient = await _db
                            .Users
                            .FirstOrDefaultAsync(u => u.Id == request.RecipientId);

        if (recipient is null)
            throw new UserNotFoundException(message: "User not found");

        request.SenderId = _userInfo.UserId!;
        request.Recipient = recipient;

        var requestSender = await _userManager.FindByIdAsync(request.SenderId);

        request.Sender = requestSender!;

        // process the connect request
        var entityEntry = await _db
                                .ContactRequests
                                .AddAsync(request);

        if (entityEntry.State is EntityState.Added)
        {
            await _db.SaveChangesAsync();

            _logger.LogInformation
            (
                "{username} sent contact request to {recipient}",
                _userInfo.Name,
                recipient.UserName
            );

            // * Notify the recipient of the request
            var notifyRecipientTask = _contactsHubContext
                                        .Clients
                                        .User(request.RecipientId)
                                        .ReceiveContactRequestAsync
                                        (
                                            request.ToPendingContactRequestIncomingDto()
                                        );

            var notifySenderTask = _contactsHubContext
                                        .Clients
                                        .User(request.SenderId)
                                        .ReceiveSentContactRequestAsync
                                        (
                                            request.ToPendingContactRequestSentDto()
                                        );

            await Task.WhenAll(notifyRecipientTask, notifySenderTask);
            return;
        }

        _logger.LogError
        (
            "Connect request between {username} and {recipient} failed",
            _userInfo.Name,
            recipient.UserName
        );

        throw new ContactRequestOperationFailureException(message: "Something went wrong while sending request");
    }


    public async Task CancelConnectRequestAsync(Guid requestId)
    {
        var request = await _db
                            .ContactRequests
                            .FindAsync(requestId);

        if (request is null)
            throw new ResourceNotFoundException(message: "Request not found");

        var deletionResult = _db.ContactRequests.Remove(request);

        if (deletionResult.State is EntityState.Deleted)
        {
            _logger.LogInformation
            (
                "{senderId} cancelled contact request with {recipientId}",
                request.SenderId,
                request.RecipientId
            );

            await _db.SaveChangesAsync();

            var notifyRecipientTask = _contactsHubContext
                                        .Clients
                                        .Users(request.RecipientId)
                                        .ReceiveCancelledRequestIdForRecipientAsync(requestId);

            var notifySenderTask = _contactsHubContext
                                        .Clients
                                        .Users(request.SenderId)
                                        .ReceiveCancelledRequestIdForSenderAsync(requestId);

            await Task.WhenAll(notifyRecipientTask, notifySenderTask);
            return;
        }

        throw new ContactRequestOperationFailureException(message: "Something went wrong while cancelling the request");
    }


    public async Task ResolveConnectRequestAsync(Guid requestId, RequestStatus newStatus)
    {
        var request = await _db
                            .ContactRequests
                            .Include(p => p.Sender)
                            .Include(p => p.Recipient)
                            .FirstAsync(req => req.Id == requestId);

        if (request is null)
            throw new ResourceNotFoundException(message: "The request you're looking for was not found");

        if (request.Status is RequestStatus.Declined || request.Status is RequestStatus.Accepted)
            throw new InvalidContactOperationException(message: "Request already responded to");

        if (newStatus is RequestStatus.Accepted)
        {
            // new up a thread for the users involved in the contact request
            var thread = new ChatThread()
            {
                Id = Guid.NewGuid(),
                Participants = new List<AppUser> { request.Sender, request.Recipient },
                CreatedAt = DateTime.UtcNow
            };

            request.Status = RequestStatus.Accepted;
            request.ChatThreadId = thread.Id;
            request.ChatThread = thread;

            await _db.Threads.AddAsync(thread);

            _logger.LogInformation
            (
                "{recipientName} accepted contact request from {sender} & new chat thread was created: Thread ID: {threadId}",
                request.Recipient.UserName,
                request.Sender.UserName,
                thread.Id
            );

            await _db.SaveChangesAsync();

            var notifyRecipientTask = _contactsHubContext
                                        .Clients
                                        .User(request.RecipientId)
                                        .ReceiveNewContactAsync
                                        (
                                            new NewContactDto
                                            {
                                                ThreadId = thread.Id,
                                                Contact = request.Sender.ToUserDetailsDto()
                                            }
                                        );

            var notifySenderTask = _contactsHubContext
                                        .Clients
                                        .User(request.SenderId)
                                        .ReceiveNewContactAsync
                                        (
                                            new NewContactDto
                                            {
                                                ThreadId = thread.Id,
                                                Contact = request.Recipient.ToUserDetailsDto()
                                            }
                                        );

            var notifySenderAboutAcceptedReqTask = _contactsHubContext
                                        .Clients
                                        .User(request.SenderId)
                                        .ReceiveAcceptedRequestId(requestId);

            await Task.WhenAll(notifyRecipientTask, notifySenderTask);

            return;
        }
        else
        {
            // in case the request was declined, remove it from the db
            var deletionResult = _db.ContactRequests.Remove(request);

            if (deletionResult.State is EntityState.Deleted)
            {
                _logger.LogInformation
                (
                    "{recipientName} declined contact request with {sender}",
                    request.Recipient.UserName,
                    request.Sender.UserName
                );

                await _db.SaveChangesAsync();

                await _contactsHubContext
                    .Clients
                    .User(request.SenderId)
                    .ReceiveCancelledRequestIdForSenderAsync(requestId);

                return;
            }
        }
    }


    public async Task<IEnumerable<ContactRequest>> GetPendingContactRequestsAsync(UserType userType)
    {
        var pendingRequests = userType switch
        {
            // * Requests Sent
            UserType.Sender => await _db
                                    .ContactRequests
                                    .Where(req => req.SenderId == _userInfo.UserId)
                                    .Where(req => req.Status == RequestStatus.Pending)
                                    .ToListAsync(),

            // * Requests Received
            UserType.Recipient => await _db
                                    .ContactRequests
                                    .Where(req => req.RecipientId == _userInfo.UserId)
                                    .Where(req => req.Status == RequestStatus.Pending)
                                    .ToListAsync(),

            _ => throw new ArgumentException("Invalid user type")
        };

        return pendingRequests;
    }


    public async Task<Dictionary<Guid, AppUser>> GetUserContactsAsync()
    {
        Dictionary<Guid, AppUser> contacts = new();

        var acceptedRequests = await _db
                                    .ContactRequests
                                    .Where
                                    (
                                        req => req.Status == RequestStatus.Accepted &&
                                        (req.SenderId == _userInfo.UserId || req.RecipientId == _userInfo.UserId)
                                    )
                                    .Include(req => req.ChatThread)
                                    .ToListAsync();

        foreach (var request in acceptedRequests)
        {
            var otherParticipantId = request.SenderId == _userInfo.UserId ? request.RecipientId : request.SenderId;
            var otherParticipant = await _userManager.FindByIdAsync(otherParticipantId);

            if (otherParticipant is not null)
            {
                contacts.Add((Guid)request.ChatThreadId!, otherParticipant);
            }
        }

        return contacts;
    }

    public async Task<IEnumerable<ContactRequest>> GetContactRequestsInvolvingUserAsync(CancellationToken cancellationToken)
    {
        var contactRequests = await _db
                                    .ContactRequests // * include every request made by or sent to this user, regardless of whether the request is resolved or pending
                                    .Where(cr => (cr.SenderId == _userInfo.UserId || cr.RecipientId == _userInfo.UserId))
                                    .ToListAsync(cancellationToken);

        return contactRequests;
    }
}
