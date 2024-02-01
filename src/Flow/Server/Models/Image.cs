using Flow.Shared.Enums;

namespace Flow.Server.Models;

public class Image
{
    public Guid Id { get; set; }
    public string? AppUserId { get; set; }
    public required string Url { get; set; }
    public ImageType Type { get; set; }
}
