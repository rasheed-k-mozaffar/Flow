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
                .Skip(loadNumber * USERS_CHUNK_SIZE)
                .Take(USERS_CHUNK_SIZE)
                .ToListAsync(cancellationToken);

        var userContacts = await _contactsRepository.GetUserContactsAsync();

        var responseBody = DistinguishContactsFromNonContactsForSearchedUsers(searchResults, userContacts);

        return Ok(new ApiResponse<ICollection<UserSearchResultDto>>
        {
            Message = responseBody.Any() ? $"Found {responseBody.Count} results" : "No results found",
            Body = responseBody,
            IsSuccess = true
        });
    }


    private ICollection<UserSearchResultDto> DistinguishContactsFromNonContactsForSearchedUsers(IEnumerable<AppUser> users, IEnumerable<ContactRequest> userContacts)
    {
        var output = new List<UserSearchResultDto>();

        foreach (var user in users)
        {
            var contact = FindContactInCurrentUserContacts(userContacts, user);

            if (contact is null)
            {
                output.Add(user.ToUserSearchResultDto(Shared.Enums.SearchedUserType.NonContact));
            }
            else
            {
                output.Add(user.ToUserSearchResultDto(Shared.Enums.SearchedUserType.Contact));
            }
        }

        return output.OrderByDescending(p => p.UserType).ToList();
    }

    private ContactRequest? FindContactInCurrentUserContacts
    (
        IEnumerable<ContactRequest> userContacts,
        AppUser user
    ) =>
    userContacts.FirstOrDefault(contact => contact.SenderId == user.Id || contact.RecipientId == user.Id);
}
