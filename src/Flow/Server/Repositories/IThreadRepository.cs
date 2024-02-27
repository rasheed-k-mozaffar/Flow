namespace Flow.Server.Repositories;

public interface IThreadRepository
{
    Task<Dictionary<string, List<MessageDto>>> GetPreliminaryMessagesForUserChatThreads();
}

