namespace Flow.Shared.DataTransferObjects;

public class CreateGroupRequest
{
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public ICollection<string> Participants { get; set; } = new List<string>();
}
