using System.Net.Http.Json;
using Flow.Shared.ApiResponses;

namespace Flow.Client.Services;

public class ThreadsService : IThreadsService
{
    private const string BASE_URL = "api/threads";
    private const string GET_LATEST_MESSAGES_URL = "get-latest-messages";
    private readonly HttpClient _httpClient;

    public ThreadsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<Dictionary<string, List<MessageDto>>>> GetPreliminaryThreadsDetailsForUserAsync()
    {
        string url = $"/{BASE_URL}/{GET_LATEST_MESSAGES_URL}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);

        var results = await response.Content.ReadFromJsonAsync<ApiResponse<Dictionary<string, List<MessageDto>>>>();
        return results!;
    }
}
