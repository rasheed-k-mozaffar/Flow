namespace Flow.Shared.DataTransferObjects;

public class GroupDetailsResponse
{
    public Guid GroupThreadId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? GroupPictureUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<UserDetailsDto> GroupParticipants { get; set; } = new List<UserDetailsDto>();
}
