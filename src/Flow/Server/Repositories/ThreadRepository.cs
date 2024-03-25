using Flow.Shared.Enums;

namespace Flow.Server.Repositories;

public class ThreadRepository : IThreadRepository
{
    private const int DEFAULT_MESSAGES_LOAD_SIZE = 20;
    private int messageLoadSize = 20;
    private readonly AppDbContext _db;
    private readonly IContactRequestsRepository _contactsRepository;

    public ThreadRepository(AppDbContext db, IContactRequestsRepository contactsRepository)
    {
        _db = db;
        _contactsRepository = contactsRepository;
    }

    public async Task<Dictionary<Guid, ChatDetails>> GetChatThreadsAsync()
    {
        var threadsAndInitialMessages = new Dictionary<Guid, ChatDetails>();

        var userContacts = await _contactsRepository.GetUserContactsAsync();

        foreach (var contact in userContacts)
        {

            var threadMessages = await _db
                                    .Messages
                                    .Where(message => message.ThreadId == contact.Key)
                                    .OrderByDescending(message => message.SentOn)
                                    .Take(DEFAULT_MESSAGES_LOAD_SIZE)
                                    .Select(msg => msg.ToMessageDto())
                                    .ToListAsync();
            threadMessages.Reverse();

            threadsAndInitialMessages.Add(contact.Key, new ChatDetails
            {
                ChatThreadId = contact.Key,
                Messages = threadMessages,
                Contact = contact.Value.ToUserDetailsDto()
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

