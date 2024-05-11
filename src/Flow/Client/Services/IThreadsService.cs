using Flow.Shared.ApiResponses;

namespace Flow.Client.Services;

public interface IThreadsService
{
    Task<ApiResponse<Dictionary<Guid, ChatDetails>>> GetChatsAsync();
    Task<ApiResponse<PreviousMessagesResponse>> LoadPreviousMessagesAsync(LoadPreviousMessagesRequest request);
    Task<ApiResponse<LoadChatMediaResponse>> GetChatMediaAsync(LoadChatMediaRequest request, CancellationToken cancellationToken);
}
