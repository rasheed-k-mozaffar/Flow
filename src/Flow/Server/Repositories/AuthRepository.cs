using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Flow.Server.Repositories;

public class AuthRepository : IAuthRepository
{
    #region Dependencies
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<AuthRepository> _logger;
    private readonly JwtOptions _jwtSettings;

    public AuthRepository
    (
        UserManager<AppUser> userManager,
        ILogger<AuthRepository> logger,
        JwtOptions jwtSettings
    )
    {
        _userManager = userManager;
        _logger = logger;
        _jwtSettings = jwtSettings;
    }
    #endregion

    /// <summary>
    /// Signs in the user if the credentials were valid, if the user is locked out, notify them by sending
    /// back the date on which the lockout ends
    /// </summary>
    /// <param name="request">User credentials</param>
    /// <returns></returns>
    /// <exception cref="AuthFailedException"></exception>
    public async Task<UserManagerResponse> LoginUserAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email!);

        if (user is null)
            throw new AuthFailedException(message: "Invalid credentials");

        // check if the user is lockedout
        if (!await _userManager.IsLockedOutAsync(user)) // uesr not locked out
        {
            var correctPassword = await _userManager
                .CheckPasswordAsync(user, request.Password!);

            if (correctPassword)
            {
                var token = GenerateJwtToken(user);

                _logger.LogInformation("User {email} signed in successfully", user.Email);

                return new UserManagerResponse
                {
                    Message = "Successful sign in",
                    Token = token,
                    Succeeded = true
                };
            }
            else
            {
                // increment the failed access attempts
                await _userManager.AccessFailedAsync(user);

                _logger.LogInformation("User {email} user incorrect password", user.Email);

                return new UserManagerResponse
                {
                    Message = "Email or password is incorrect",
                    Token = null,
                    Succeeded = false,
                    IsLockedOut = false
                };
            }
        }
        else // user locked out
        {
            var lockoutEndsIn = await _userManager
                        .GetLockoutEndDateAsync(user);

            _logger.LogInformation("User {email} attempted login while on lockout", user.Email);

            return new UserManagerResponse
            {
                Message = $"Too many login attempts, try again on {lockoutEndsIn}",
                Token = null,
                Succeeded = false,
                IsLockedOut = true,
            };
        }
    }

    public async Task<UserManagerResponse> RegisterUserAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email!);

        if (existingUser is not null)
            throw new UserCreationFailedException(message: "Email is taken");

        var newUser = new AppUser()
        {
            FirstName = request.FirstName!,
            LastName = request.LastName!,
            DateOfBirth = request.DateOfBirth,
            UserName = request.Username,
            Email = request.Email
        };

        var userCreationResult = await _userManager.CreateAsync(newUser, request.Password!);

        // if the registration succeeded, give the user an access token directly
        if (userCreationResult.Succeeded)
        {
            var token = GenerateJwtToken(newUser);

            _logger.LogInformation
            (
                "New user {username} with email {email} was registered and signed in",
                newUser.UserName,
                newUser.Email
            );

            return new UserManagerResponse
            {
                Message = "Your account has been created",
                Token = token,
                Succeeded = true
            };
        }

        throw new UserCreationFailedException(message: "Something went wrong while attempting to sign you up");
    }


    /// <summary>
    /// Generates a JWT token with a defined set of claims for the given user
    /// </summary>
    /// <param name="user">The owner of the token</param>
    /// <returns>The JWT as a string</returns>
    private string GenerateJwtToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
        var expiresIn = DateTime.UtcNow.AddDays(_jwtSettings.ExpiresIn);

        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer),
            new Claim(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience),
            new Claim(JwtRegisteredClaimNames.Exp, expiresIn.ToString())
        ];

        JwtSecurityToken token = new
        (
            audience: _jwtSettings.Audience,
            issuer: _jwtSettings.Issuer,
            expires: expiresIn,
            claims: claims,
            signingCredentials: signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
