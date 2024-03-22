namespace Flow.Shared.DataTransferObjects;

public class ColorSchemeDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? SentMsgBubbleColor { get; set; }
    public string? ReceivedMsgBubbleColor { get; set; }
    public AccentsColor? AccentsColor { get; set; }
    public string? SelectedMessageColor { get; set; }
}
