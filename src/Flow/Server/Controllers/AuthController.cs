using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthRepository _authRepository;

    public AuthController
    (
        ILogger<AuthController> logger,
        IAuthRepository authRepository
    )
    {
        _logger = logger;
        _authRepository = authRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (ModelState.IsValid)
        {
            var loginResult = await _authRepository
                                    .LoginUserAsync(request);

            if (loginResult.Succeeded)
            {
                return Ok(new ApiResponse<string>
                {
                    Message = loginResult.Message,
                    Body = loginResult.Token,
                    IsSuccess = true
                });
            }

            // user cannot login due to many login attempts
            // the lockout end date is sent back to the client
            if (loginResult.IsLockedOut)
            {
                return Ok(new ApiErrorResponse
                {
                    ErrorMessage = loginResult.Message
                });
            }

            // user is not locked out, but failed to log in
            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = loginResult.Message
            });

        }
        else
        {
            return BadRequest(ModelState);
        }
    }

}
