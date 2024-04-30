using System.ComponentModel.DataAnnotations;

namespace Flow.Shared.DataTransferObjects;

public class CreateGroupRequest
{
    public Guid GroupThreadId { get; set; } = Guid.NewGuid();
    [Required(ErrorMessage = "The group name is required")]
    public string? GroupName { get; set; }
    public string? Description { get; set; }
    public string? GroupPictureUrl { get; set; }
    public ICollection<string> Participants { get; set; } = new List<string>();
}
