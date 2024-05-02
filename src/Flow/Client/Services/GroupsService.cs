using System.Net.Http.Json;
using Flow.Shared.ApiResponses;

namespace Flow.Client.Services;

public class GroupsService : IGroupsService
{
    private const string BASE_URL = "/api/groups";
    private const string CREATE_GROUP_URL = $"{BASE_URL}/create-group";
    private readonly HttpClient _httpClient;

    public GroupsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<ApiResponse> AddNewMembersToGroupAsync(AddNewGroupParticipantsRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiResponse> CreateGroupAsync(CreateGroupRequest request)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(CREATE_GROUP_URL, request);

        if (!response.IsSuccessStatusCode)
        {
            ApiErrorResponse? errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new OperationFailureException(errorResponse!.ErrorMessage);
        }

        ApiResponse? result = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return result!;
    }

    public Task<ApiResponse> DeleteGroupAsync(Guid groupId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse<GroupDetailsResponse>> GetGroupsDetailsAsync(Guid groupId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ApiResponse> RemoveFromGroupAsync(Guid groupId, string participantId)
    {
        throw new NotImplementedException();
    }
}
