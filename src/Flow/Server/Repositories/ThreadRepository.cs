﻿namespace Flow.Server.Repositories;

public class ThreadRepository : IThreadRepository
{
    private const int MESSAGES_LOAD_SIZE = 20;
    private readonly AppDbContext _db;
    private readonly ContactRequestsRepository _contactsRepository;

    public ThreadRepository(AppDbContext db, ContactRequestsRepository contactsRepository)
    {
        _db = db;
        _contactsRepository = contactsRepository;
    }

    public async Task<Dictionary<string, List<MessageDto>>> GetPreliminaryMessagesForUserChatThreads()
    {
        var messagesOfEachThread = new Dictionary<string, List<MessageDto>>();

        var userContacts = await _contactsRepository.GetUserContactsAsync();

        foreach (var contact in userContacts)
        {
            string threadId = contact.ChatThreadId.ToString()!;

            var threadMessages = await _db
                                    .Messages
                                    .Where(message => message.ThreadId == contact.ChatThreadId)
                                    .OrderByDescending(message => message.SentOn)
                                    .Take(MESSAGES_LOAD_SIZE)
                                    .Select(msg => msg.ToMessageDto())
                                    .ToListAsync();

            messagesOfEachThread.Add(threadId, threadMessages);
        }

        return messagesOfEachThread;
    }
}

