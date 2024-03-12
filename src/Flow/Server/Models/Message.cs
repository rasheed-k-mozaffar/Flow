using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Flow.Shared.Enums;

namespace Flow.Server.Models;

public class Message
{
    [Key]
    public Guid Id { get; set; }

    public Guid ThreadId { get; set; }

    public virtual ChatThread Thread { get; set; } = null!;

    public string SenderId { get; set; } = null!;

    public virtual AppUser Sender { get; set; } = null!;

    public required string Content { get; set; }

    public MessageType Type { get; set; }

    public DateTime SentOn { get; set; }

    public MessageStatus Status { get; set; }
}
