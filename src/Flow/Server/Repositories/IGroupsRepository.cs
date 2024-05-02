namespace Flow.Server.Repositories;

public interface IGroupsRepository
{
    Task<ChatThread> CreateGroupAsync(ChatThread groupDetails, ICollection<string> participants, string? groupPictureUrl = null);
    Task LeaveGroupAsync(Guid groupThreadId, string participantId);
    Task AddNewParticipantsToGroupAsync(Guid groupThreadId, ICollection<string> newParticipants);
    Task DeleteGroupAsync(Guid groupThreadId, CancellationToken cancellationToken);
    Task UpdateGroupDetailsAsync(Guid groupThreadId, UpdateGroupDetailsRequest request);

    // * FOR TESTING ONLY
    Task<GroupDetailsResponse> GetGroupDetailsAsync(Guid groupThreadId, CancellationToken cancellationToken);
}
