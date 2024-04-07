namespace Flow.Client.Models;

public class SentImageDetails
{
    public required string UserName { get; set; }
    public required string ImageUrl { get; set; }
    public string? UserProfilePictureUrl { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsSentByCurrentUser { get; set; }
}
