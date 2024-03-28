using Flow.Shared.ApiResponses;

namespace Flow.Client.Services;

public interface IUsersService
{
    Task<ApiResponse<UsersSearchResultsResponse>> SearchAsync(string searchTerm, CancellationToken cancellationToken, int loadNumber = 0);
}
