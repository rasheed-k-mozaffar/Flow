using Flow.Shared.Enums;

namespace Flow.Server.Helpers;

public static class UserDataTransferObjectsMapper
{
    /// <summary>
    /// Maps an AppUser object to a minimal UserDetailsDto object
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static UserDetailsDto ToUserDetailsDto(this AppUser user)
    {
        return new()
        {
            UserId = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            Username = user.UserName,
            ProfilePictureUrl = user.ProfilePicture?.RelativeUrl // * in case user has a profile picture
        };
    }


    /// <summary>
    /// Maps an AppUser object to a UserSearchResult object that'll be used when returning search results for
    /// the user performing the search
    /// </summary>
    /// <param name="user"></param>
    /// <param name="searchedUserType"></param>
    /// <returns></returns>
    public static UserSearchResultDto ToUserSearchResultDto(this AppUser user, SearchedUserType searchedUserType)
    {
        return new()
        {
            UserId = user.Id,
            Username = user.UserName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Bio = user.Bio,
            ProfilePictureUrl = user.ProfilePicture?.RelativeUrl,
            UserType = searchedUserType
        };
    }
}
