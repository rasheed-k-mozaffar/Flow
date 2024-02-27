namespace Flow.Server.Repositories
{
    public class ThreadRepository : IThreadRepository
    {
        private readonly AppDbContext _DbContext;
        private readonly ContactRequestsRepository _CrRepo;
        public ThreadRepository(AppDbContext AppDbContext, ContactRequestsRepository ContactRequestsRepository) {
            _DbContext = AppDbContext;
            _CrRepo = ContactRequestsRepository;
        }
        public async Task<Dictionary<string, List<Message>>> GetPreliminaryMessages() 
        {
            var messagesOfEachThread = new Dictionary<string, List<Message>>();

            var userThreads = await _CrRepo.GetUserContactsAsync();
            foreach(var thread in userThreads)
            {
                string threadId = thread.ChatThreadId.ToString()!;
                var ThreadMessages = await _DbContext.Messages.Where(message => message.ThreadId == thread.Id)
                    .OrderByDescending(message => message.SentOn)
                    .Take(15)
                    .ToListAsync();
                messagesOfEachThread.Add(threadId, ThreadMessages);
            }

            return messagesOfEachThread;
        }
        public async Task<Dictionary<string, List<MessageDto>>> GetPreliminaryMessagesDTO()
        {
            var messagesOfEachThread = new Dictionary<string, List<MessageDto>>();
            var userThreads = await _CrRepo.GetUserContactsAsync();
            foreach (var thread in userThreads)
            {
                string threadId = thread.ChatThreadId.ToString()!;
                var ThreadMessages = await _DbContext.Messages.Where(message => message.ThreadId == thread.Id)
                    .OrderByDescending(message => message.SentOn)
                    .Take(15)
                    .Select(m => m.ToMessageDto())
                    .ToListAsync();
                messagesOfEachThread.Add(threadId, ThreadMessages);
            }

            return messagesOfEachThread;
        }
    }
}
