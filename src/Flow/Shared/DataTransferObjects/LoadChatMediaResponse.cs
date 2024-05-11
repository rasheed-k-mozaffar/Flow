namespace Flow.Shared.DataTransferObjects;

public class LoadChatMediaResponse
{
    public List<MessageDto> Media { get; set; } = new();
    public int TotalItems { get; set; }
    public int RemainingItems { get; set; }
    public bool HasLoadedAll => RemainingItems <= 0;
}