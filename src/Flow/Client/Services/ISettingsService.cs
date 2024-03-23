using Flow.Shared.ApiResponses;

namespace Flow.Client.Services;

public interface ISettingsService
{
    Task<ApiResponse<UserSettingsDto>> GetUserSettingsAsync();
    Task<ApiResponse<IEnumerable<ColorSchemeDto>>> GetColorSchemesAsync();
    Task<ApiResponse> UpdateSettingsAsync(UserSettingsDto newSettings);
}
