using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly IUserSettingsRepository _userSettingsRepo;
    private readonly UserInfo _userInfo;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController
    (
        IUserSettingsRepository userSettingsRepo,
        UserInfo userInfo,
        ILogger<SettingsController> logger
    )
    {
        _userSettingsRepo = userSettingsRepo;
        _userInfo = userInfo;
        _logger = logger;
    }

    [HttpGet("get-settings")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse<UserSettingsDto>))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(UnauthorizedResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, type: typeof(ApiErrorResponse))]
    public async Task<IActionResult> GetSettings()
    {
        try
        {
            var settings = await _userSettingsRepo.GetUserSettingsAsync(_userInfo.UserId!);

            return Ok(new ApiResponse<UserSettingsDto>
            {
                Message = "Settings retrieved successfully",
                Body = settings.ToSettingsDto(),
                IsSuccess = true
            });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }
}
