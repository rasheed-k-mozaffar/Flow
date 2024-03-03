using System.Net.Http.Json;
using Flow.Shared.ApiResponses;

namespace Flow.Client.Services;

public class MessagesService : IMessagesService
{
    private const string BASE_URL = "/api/threads";
    private const string DELETE_MESSAGES_URL = $"{BASE_URL}/delete-messages";
    private readonly HttpClient _httpClient;

    public MessagesService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task SendDeleteMessagesRequestAsync(DeleteMessagesRequest request)
    {
        string url = $"{DELETE_MESSAGES_URL}";
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, request);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new OperationFailureException(errorResponse!.ErrorMessage!);
        }
    }
}
