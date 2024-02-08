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
        return new UserDetailsDto
        {
            UserId = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            Username = user.UserName,
            ProfilePictureUrl = user.ProfilePicture?.RelativeUrl // * in case user has a profile picture
        };
    }
}
