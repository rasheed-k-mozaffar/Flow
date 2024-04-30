using Flow.Shared.Enums;

namespace Flow.Server.Models;

public class Image
{
    public Guid Id { get; set; }
    public Guid? ChatThreadId { get; set; }
    public string? AppUserId { get; set; }
    public required string RelativeUrl { get; set; }
    public required string FilePath { get; set; }
    public ImageType Type { get; set; }
}
