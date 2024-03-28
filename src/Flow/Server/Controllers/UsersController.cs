using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private const int USERS_CHUNK_SIZE = 10;
    private readonly ILogger<UsersController> _logger;
    private readonly UserManager<AppUser> _usersManager;
    private readonly UserInfo _userInfo;
    private readonly IContactRequestsRepository _contactsRepository;

    public UsersController(ILogger<UsersController> logger, UserManager<AppUser> usersManager, UserInfo userInfo, IContactRequestsRepository contactsRepository)
    {
        _logger = logger;
        _usersManager = usersManager;
        _userInfo = userInfo;
        _contactsRepository = contactsRepository;
    }

    [HttpGet]
    [Route("search-users")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse<ICollection<UserSearchResultDto>>))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(UnauthorizedResult))]
    public async Task<IActionResult> SearchForUsers([FromQuery] string searchTerm, [FromQuery] int loadNumber, CancellationToken cancellationToken)
    {
        string searchTermLowercase = searchTerm.ToLower();

        var searchResults = await _usersManager
                .Users
                .Where(user =>
                user.Id != _userInfo.UserId && // * exclude the user performing the search
                (user.UserName!.ToLower().Contains(searchTermLowercase) ||
                user.FirstName.ToLower().Contains(searchTermLowercase) ||
                user.LastName.ToLower().Contains(searchTermLowercase)))
                .AsSplitQuery()
                .OrderBy(u => u.FirstName)
                .Skip(loadNumber * USERS_CHUNK_SIZE)
                .Take(USERS_CHUNK_SIZE)
                .ToListAsync(cancellationToken);

        var unloadedUsersCount = await CountUnloadedSearchResultsAsync(searchTermLowercase, cancellationToken, loadNumber);

        var userContactRequests = await _contactsRepository.GetContactRequestsInvolvingUserAsync(cancellationToken);

        var distinguishedUsers = DistinguishSearchedUsersType(searchResults, userContactRequests);
        var responseBody = new UsersSearchResultsResponse
        {
            SearchResults = distinguishedUsers,
            UnloadedUsersCount = unloadedUsersCount,
            HasUnloadedUsers = unloadedUsersCount > USERS_CHUNK_SIZE
        };

        return Ok(new ApiResponse<UsersSearchResultsResponse>
        {
            Message = distinguishedUsers.Any() ? $"Found {distinguishedUsers.Count} results for \"{searchTerm}\"" : $"No results found for \"{searchTerm}\"",
            Body = responseBody,
            IsSuccess = true
        });
    }


    private ICollection<UserSearchResultDto> DistinguishSearchedUsersType(IEnumerable<AppUser> users, IEnumerable<ContactRequest> userContactRequests)
    {
        var output = new List<UserSearchResultDto>();

        foreach (var user in users)
        {
            var contact = FindContactInRequestsInvolvingTheUser(userContactRequests, user);

            if (contact is null)
            {
                output.Add(user.ToUserSearchResultDto(Shared.Enums.SearchedUserType.NonContact, null));
            }
            else if (contact.Status is Shared.Enums.RequestStatus.Pending)
            {
                output.Add(user.ToUserSearchResultDto(Shared.Enums.SearchedUserType.PendingRequest, contact.Id));
            }
            else
            {
                output.Add(user.ToUserSearchResultDto(Shared.Enums.SearchedUserType.Contact, contact.Id));
            }
        }

        return output.OrderByDescending(p => p.UserType).ToList();
    }

    // looks through the user's contact requests and checks to see if the user is involved or not
    private ContactRequest? FindContactInRequestsInvolvingTheUser
    (
        IEnumerable<ContactRequest> userContactRequests,
        AppUser user
    ) =>
    userContactRequests.FirstOrDefault(cr => cr.SenderId == user.Id || cr.RecipientId == user.Id);

    private async Task<int> CountUnloadedSearchResultsAsync(string searchTermLowercase, CancellationToken cancellationToken, int loadNumber)
    {
        var count = await _usersManager
                        .Users
                        .Skip(loadNumber * USERS_CHUNK_SIZE)
                        .CountAsync(user =>
                        user.Id != _userInfo.UserId &&
                        (user.UserName!.ToLower().Contains(searchTermLowercase) ||
                        user.FirstName.ToLower().Contains(searchTermLowercase) ||
                        user.LastName.ToLower().Contains(searchTermLowercase)), cancellationToken);

        return count;
    }
}
