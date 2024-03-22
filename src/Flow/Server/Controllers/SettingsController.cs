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
    private readonly IColorSchemesRepository _schemesRepository;
    private readonly UserInfo _userInfo;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController
    (
        IUserSettingsRepository userSettingsRepo,
        UserInfo userInfo,
        ILogger<SettingsController> logger,
        IColorSchemesRepository schemesRepository
    )
    {
        _userSettingsRepo = userSettingsRepo;
        _userInfo = userInfo;
        _logger = logger;
        _schemesRepository = schemesRepository;
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

    [HttpGet("get-color-schemes")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse<IEnumerable<ColorSchemeDto>>))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(UnauthorizedResult))]
    public async Task<IActionResult> GetColorSchemes()
    {
        var schemes = await _schemesRepository.GetSchemesAsync();

        return Ok(new ApiResponse<IEnumerable<ColorSchemeDto>>
        {
            Message = "Schemes retrieved successfully",
            Body = schemes.Select(s => s.ToColorSchemeDto()),
            IsSuccess = true
        });
    }
}
