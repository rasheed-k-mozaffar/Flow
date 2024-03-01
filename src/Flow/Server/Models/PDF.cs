using Flow.Shared.Enums;

namespace Flow.Server.Models
{
    public class PDF
    {
        public Guid Id { get; set; }
        public string? AppUserId { get; set; }
        public required string RelativeUrl { get; set; }
        public required string FilePath { get; set; }
    }
}
