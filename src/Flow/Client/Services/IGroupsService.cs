using Flow.Shared.ApiResponses;

namespace Flow.Client;

public interface IGroupsService
{
    Task<ApiResponse<GroupDetailsResponse>> GetGroupsDetailsAsync(Guid groupId, CancellationToken cancellationToken);
    Task<ApiResponse> CreateGroupAsync(CreateGroupRequest request);
    Task<ApiResponse> AddNewMembersToGroupAsync(AddNewGroupParticipantsRequest request);
    Task<ApiResponse> RemoveFromGroupAsync(Guid groupId, string participantId);
    Task<ApiResponse> DeleteGroupAsync(Guid groupId, CancellationToken cancellationToken);
}
