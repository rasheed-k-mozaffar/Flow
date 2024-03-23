using System.Net.Http.Json;
using Flow.Shared.ApiResponses;

namespace Flow.Client.Services;

public class SettingsService : ISettingsService
{
    private const string BASE_URL = "/api/settings";
    private const string GET_SETTINGS_URL = $"{BASE_URL}/get-settings";
    private const string GET_SCHEMES_URL = $"{BASE_URL}/get-color-schemes";
    private const string UPDATE_SETTINGS_URL = $"{BASE_URL}/update-settings";

    private readonly HttpClient _httpClient;

    public SettingsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<IEnumerable<ColorSchemeDto>>> GetColorSchemesAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(GET_SCHEMES_URL);

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ColorSchemeDto>>>();
        return data!;
    }

    public async Task<ApiResponse<UserSettingsDto>> GetUserSettingsAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(GET_SETTINGS_URL);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new ApiGetRequestFailedException(errorResponse!.ErrorMessage);
        }

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<UserSettingsDto>>();
        return data!;
    }

    public async Task<ApiResponse> UpdateSettingsAsync(UserSettingsDto newSettings)
    {
        HttpResponseMessage response = await _httpClient
                                            .PostAsJsonAsync(UPDATE_SETTINGS_URL, newSettings);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new OperationFailureException(errorResponse!.ErrorMessage);
        }

        var result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return result!;
    }
}
