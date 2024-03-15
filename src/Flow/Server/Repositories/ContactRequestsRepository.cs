using Flow.Shared.Enums;

namespace Flow.Server.Repositories;

public class ContactRequestsRepository : IContactRequestsRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<ContactRequestsRepository> _logger;
    private readonly UserInfo _userInfo;

    public ContactRequestsRepository
    (
        AppDbContext db,
        ILogger<ContactRequestsRepository> logger,
        UserInfo userInfo
    )
    {
        _db = db;
        _logger = logger;
        _userInfo = userInfo;
    }


    public async Task SendConnectRequestAsync(string userId, ContactRequest request)
    {
        // find the recipient
        var recipient = await _db
                            .Users
                            .FirstOrDefaultAsync(u => u.Id == request.RecipientId);

        if (recipient is null)
            throw new UserNotFoundException(message: "User not found");

        request.SenderId = _userInfo.UserId!;

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
            throw new ResourceNotFoundException(message: "The request you're looking for was not found");

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


    public async Task<IEnumerable<ContactRequest>> GetUserContactsAsync()
    {
        var contacts = await _db
                            .ContactRequests
                            .Where(cr => (cr.SenderId == _userInfo.UserId || cr.RecipientId == _userInfo.UserId) && cr.Status == RequestStatus.Accepted)
                            .ToListAsync();

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
