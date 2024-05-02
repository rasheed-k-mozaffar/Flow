using Flow.Shared.Enums;

namespace Flow.Shared.DataTransferObjects;

public class ChatDetails
{
    public Guid ChatThreadId { get; set; }
    public List<MessageDto> Messages { get; set; } = new();
    public List<UserDetailsDto> Participants { get; set; } = null!;
    public ThreadType Type { get; set; }
    public string? GroupName { get; set; }
    public string? GroupDescription { get; set; }
    public string? GroupImageUrl { get; set; }
}
