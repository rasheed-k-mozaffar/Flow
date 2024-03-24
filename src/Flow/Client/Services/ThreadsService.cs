using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Http.Internal;

namespace Flow.Client.Services;

public class ThreadsService : IThreadsService
{
    private const string BASE_URL = "api/threads";
    private const string GET_LATEST_MESSAGES_URL = $"{BASE_URL}/get-latest-messages";
    private const string GET_PREVIOUS_MESSAGES_URL = $"{BASE_URL}/get-previous-messages";
    private readonly HttpClient _httpClient;

    public ThreadsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<Dictionary<string, List<MessageDto>>>> GetPreliminaryThreadsDetailsForUserAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(GET_LATEST_MESSAGES_URL);

        var results = await response
                            .Content
                            .ReadFromJsonAsync<ApiResponse<Dictionary<string, List<MessageDto>>>>();

        return results!;
    }

    public async Task<ApiResponse<PreviousMessagesResponse>> LoadPreviousMessagesAsync(LoadPreviousMessagesRequest request)
    {
        string jsonRequestBody = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient
                                        .PostAsync
                                        (
                                            GET_PREVIOUS_MESSAGES_URL,
                                            content
                                        );

        if (!response.IsSuccessStatusCode)
        {
            throw new LoadingMessagesFailedException("Previous messages couldn't be loaded");
        }

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<PreviousMessagesResponse>>();
        return data!;
    }
}
