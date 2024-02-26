using System.Runtime.ExceptionServices;
using Flow.Shared.Enums;

namespace Flow.Shared.DataTransferObjects;

public class UserSearchResultDto
{
    public Guid? ContactRequestId { get; set; }
    public string? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Name => $"{FirstName} {LastName}";
    public string? Username { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? Bio { get; set; }
    public SearchedUserType UserType { get; set; }
}
