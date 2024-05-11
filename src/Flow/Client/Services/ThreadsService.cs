using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Http.Internal;

namespace Flow.Client.Services;

public class ThreadsService : IThreadsService
{
    private const string BaseUrl = "api/threads";
    private const string GetLatestMessagesUrl = $"{BaseUrl}/get-latest-messages";
    private const string GetPreviousMessagesUrl = $"{BaseUrl}/get-previous-messages";
    private const string GetChatMediaUrl = $"{BaseUrl}/get-media";
    private readonly HttpClient _httpClient;

    public ThreadsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<Dictionary<Guid, ChatDetails>>> GetChatsAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(GetLatestMessagesUrl);

        var results = await response
                            .Content
                            .ReadFromJsonAsync<ApiResponse<Dictionary<Guid, ChatDetails>>>();

        return results!;
    }

    public async Task<ApiResponse<PreviousMessagesResponse>> LoadPreviousMessagesAsync(LoadPreviousMessagesRequest request)
    {
        string jsonRequestBody = JsonSerializer.Serialize(request);
        StringContent content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _httpClient
                                        .PostAsync
                                        (
                                            GetPreviousMessagesUrl,
                                            content
                                        );

        if (!response.IsSuccessStatusCode)
        {
            throw new LoadingMessagesFailedException("Previous messages couldn't be loaded");
        }

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<PreviousMessagesResponse>>();
        return data!;
    }

    public async Task<ApiResponse<LoadChatMediaResponse>> GetChatMediaAsync(LoadChatMediaRequest request, CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _httpClient
                .PostAsJsonAsync(GetChatMediaUrl, request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            ApiErrorResponse? errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(cancellationToken: cancellationToken);
            throw new ApiGetRequestFailedException(errorResponse!.ErrorMessage);
        }

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<LoadChatMediaResponse>>(cancellationToken: cancellationToken);
        return data!;
    }
}
