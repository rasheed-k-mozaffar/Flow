using Flow.Shared.Enums;

namespace Flow.Server.Repositories;

public class ThreadRepository : IThreadRepository
{
    private const int DefaultMessagesLoadSize = 20;
    private int _messageLoadSize = 20;
    private readonly AppDbContext _db;
    private readonly IContactRequestsRepository _contactsRepository;
    private readonly UserInfo _userInfo;

    public ThreadRepository(AppDbContext db, IContactRequestsRepository contactsRepository, UserInfo userInfo)
    {
        _db = db;
        _contactsRepository = contactsRepository;
        _userInfo = userInfo;
    }

    public async Task<Dictionary<Guid, ChatDetails>> GetChatThreadsAsync()
    {
        var threadsAndInitialMessages = new Dictionary<Guid, ChatDetails>();

        // var userContacts = await _contactsRepository.GetUserContactsAsync();

        var userChatThreads = await _db
                                    .Threads
                                    .Include(p => p.Participants)
                                    .Include(p => p.GroupImage)
                                    .Where
                                    (
                                        t => t.Participants
                                        .Select(p => p.Id)
                                        .Contains(_userInfo.UserId)
                                    )
                                    .ToListAsync();

        foreach (var chatThread in userChatThreads)
        {
            var threadMessages = await _db
                                    .Messages
                                    .Where(message => message.ThreadId == chatThread.Id)
                                    .OrderByDescending(message => message.SentOn)
                                    .Take(DefaultMessagesLoadSize)
                                    .Select(msg => msg.ToMessageDto())
                                    .ToListAsync();
            threadMessages.Reverse();

            threadsAndInitialMessages.Add(chatThread.Id, new ChatDetails
            {
                ChatThreadId = chatThread.Id,
                Messages = threadMessages,
                Participants = chatThread.Participants.Select(u => u.ToUserDetailsDto()).ToList(),
                GroupName = chatThread.Name,
                GroupDescription = chatThread.Description,
                Type = chatThread.Type,
                GroupImageUrl = chatThread.GroupImage?.RelativeUrl
            });
        }

        return threadsAndInitialMessages;
    }
    public async Task<PreviousMessagesResponse> GetPreviousMessagesByDateAsync(LoadPreviousMessagesRequest request)
    {
        int unloadedMessagesCount = await GetUnloadedMessagesCount(request.ThreadId, request.LastMessageDate);
        _messageLoadSize = CalculateMessagesLoadSize(unloadedMessagesCount);

        var olderMessages = await _db
                                    .Messages
                                    .Where(m => m.ThreadId == request.ThreadId && request.LastMessageDate > m.SentOn)
                                    .AsSplitQuery()
                                    .OrderByDescending(m => m.SentOn)
                                    .Take(_messageLoadSize)
                                    .ToListAsync();

        var messageDtos = olderMessages
                            .Select(m => m.ToMessageDto())
                            .Reverse()
                            .ToList();

        return new PreviousMessagesResponse
        {
            Messages = messageDtos,
            HasUnloadedMessages = unloadedMessagesCount >= _messageLoadSize,
            UnloadedMessageCount = unloadedMessagesCount - _messageLoadSize
        };
    }

    public async Task<LoadChatMediaResponse> GetChatMediaAsync(LoadChatMediaRequest request, CancellationToken cancellationToken)
    {
        var chatThread = await _db
            .Threads
            .AsNoTracking()
            .FirstOrDefaultAsync
            (
                chat => chat.Id == request.ChatThreadId,
                cancellationToken
            );

        if (chatThread is null)
        {
            throw new ResourceNotFoundException("Chat was not found");
        }

        var chatMedia = await _db
            .Messages
            .Where(m => (m.Type == MessageType.Image && m.ThreadId == chatThread.Id))
            .OrderByDescending(p => p.SentOn)
            .Skip(request.LoadNumber * request.LoadSize)
            .Take(request.LoadSize)
            .Select(m => m.ToMessageDto())
            .ToListAsync(cancellationToken);

        var totalMediaCount = await _db
            .Messages
            .CountAsync
                (
                    m => (m.Type == MessageType.Image && m.ThreadId == chatThread.Id),
                    cancellationToken
                );

        int remainingItems = 0;

        if (request.LoadNumber == 0)
        {
            remainingItems = totalMediaCount - request.LoadSize;
        }
        else
        {
            remainingItems = totalMediaCount - (request.LoadSize * request.LoadNumber + request.LoadSize);
        }

        return new LoadChatMediaResponse()
        {
            Media = chatMedia,
            TotalItems = totalMediaCount,
            RemainingItems = remainingItems
        };
    }

    private async Task<int> GetUnloadedMessagesCount(Guid threadId, DateTime lastMessageDate)
    {
        var unloadedMsgsCount = await _db
                                    .Messages
                                    .CountAsync(m => m.ThreadId == threadId && m.SentOn < lastMessageDate);

        return unloadedMsgsCount;
    }

    private int CalculateMessagesLoadSize(int unloadedMessagesCount)
    {
        if (unloadedMessagesCount > 100)
        {
            return 40;
        }
        else if (unloadedMessagesCount < 50)
        {
            return 25;
        }
        else
        {
            return unloadedMessagesCount;
        }
    }
}

