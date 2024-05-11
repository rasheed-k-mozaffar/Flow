namespace Flow.Server.Repositories;

public interface IThreadRepository
{
    Task<Dictionary<Guid, ChatDetails>> GetChatThreadsAsync();
    Task<PreviousMessagesResponse> GetPreviousMessagesByDateAsync(LoadPreviousMessagesRequest request);
    Task<LoadChatMediaResponse> GetChatMediaAsync(LoadChatMediaRequest request, CancellationToken cancellationToken);
}

