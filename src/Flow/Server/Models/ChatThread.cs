using Flow.Shared.Enums;

namespace Flow.Server.Models;

public class ChatThread
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Name { get; set; } // * used for naming a group chat
    public string? Description { get; set; }
    public virtual Image? GroupImage { get; set; }
    public ThreadType Type { get; set; }
    public virtual ICollection<AppUser> Participants { get; set; } = new List<AppUser>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
