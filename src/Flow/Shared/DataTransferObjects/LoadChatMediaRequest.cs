namespace Flow.Shared.DataTransferObjects;

public class LoadChatMediaRequest
{
    public Guid ChatThreadId { get; set; }

    public int LoadNumber { get; set; }
    public int LoadSize { get; set; }
}