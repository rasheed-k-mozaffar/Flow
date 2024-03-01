using Flow.Shared.ApiResponses;

namespace Flow.Client.Services;

public interface IThreadsService
{
    Task<ApiResponse<Dictionary<string, List<MessageDto>>>> GetPreliminaryThreadsDetailsForUserAsync();
}
