namespace Flow.Shared.DataTransferObjects;

public class UsersSearchResultsResponse
{
    public ICollection<UserSearchResultDto> SearchResults { get; set; }
    public int UnloadedUsersCount { get; set; }
    public bool HasUnloadedUsers { get; set; }

    public UsersSearchResultsResponse()
    {
        SearchResults = new List<UserSearchResultDto>();
    }
}
