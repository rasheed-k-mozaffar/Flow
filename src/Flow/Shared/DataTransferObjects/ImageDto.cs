using Flow.Shared.Enums;

namespace Flow.Shared.DataTransferObjects;

public class ImageDto
{
    public Guid Id { get; set; }
    public string? RelativeUrl { get; set; }
    public string? AppUserId { get; set; }
    public ImageType Type { get; set; }
}
