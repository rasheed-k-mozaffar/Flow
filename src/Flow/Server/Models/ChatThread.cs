namespace Flow.Server.Models;

public class ChatThread
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual ICollection<AppUser> Participants { get; set; } = new List<AppUser>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
}
