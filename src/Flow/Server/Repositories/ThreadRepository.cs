using Flow.Shared.Enums;

namespace Flow.Server.Repositories;

public class ThreadRepository : IThreadRepository
{
    private const int DEFAULT_MESSAGES_LOAD_SIZE = 20;
    private int messageLoadSize = 20;
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
                                    .Take(DEFAULT_MESSAGES_LOAD_SIZE)
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
        messageLoadSize = CalculateMessagesLoadSize(unloadedMessagesCount);

        var olderMessages = await _db
                                    .Messages
                                    .Where(m => m.ThreadId == request.ThreadId && request.LastMessageDate > m.SentOn)
                                    .AsSplitQuery()
                                    .OrderByDescending(m => m.SentOn)
                                    .Take(messageLoadSize)
                                    .ToListAsync();

        var messageDtos = olderMessages
                            .Select(m => m.ToMessageDto())
                            .Reverse()
                            .ToList();

        return new PreviousMessagesResponse
        {
            Messages = messageDtos,
            HasUnloadedMessages = unloadedMessagesCount >= messageLoadSize,
            UnloadedMessageCount = unloadedMessagesCount - messageLoadSize
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

