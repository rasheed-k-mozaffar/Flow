namespace Flow.Shared.DataTransferObjects;

public class GroupDetailsResponse
{
    public Guid GroupdThreadId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<UserDetailsDto> GroupParticipants { get; set; } = new List<UserDetailsDto>();
}
