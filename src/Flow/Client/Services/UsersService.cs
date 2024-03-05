using System.Net.Http.Headers;
using System.Net.Http.Json;
using Flow.Shared.ApiResponses;

namespace Flow.Client.Services;

public class UsersService : IUsersService
{
    private const string BASE_URL = "/api/users";
    private const string SEARCH_URL = $"{BASE_URL}/search-users";

    private readonly HttpClient _httpClient;

    public UsersService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<ICollection<UserSearchResultDto>>> SearchAsync(string searchTerm, CancellationToken cancellationToken, int loadNumber = 0)
    {
        string requestUrl = $"{SEARCH_URL}?searchTerm={searchTerm}&loadNumber={loadNumber}";
        HttpResponseMessage response = await _httpClient.GetAsync(requestUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new ApiGetRequestFailedException(errorResponse!.ErrorMessage);
        }

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<ICollection<UserSearchResultDto>>>();
        return data!;
    }
}
