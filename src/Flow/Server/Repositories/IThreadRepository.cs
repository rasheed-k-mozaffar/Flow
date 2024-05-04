namespace Flow.Server.Repositories;

public interface IThreadRepository
{
    Task<Dictionary<Guid, ChatDetails>> GetChatThreadsAsync();
    Task<PreviousMessagesResponse> GetPreviousMessagesByDateAsync(LoadPreviousMessagesRequest request);
    Task<IEnumerable<Message>> GetChatMediaAsync(LoadChatMediaRequest request, CancellationToken cancellationToken);
}

