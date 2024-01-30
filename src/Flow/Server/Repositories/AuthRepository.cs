
using Microsoft.AspNetCore.Identity;

namespace Flow.Repositories;

public class AuthRepository : IAuthRepository
{
    #region Dependencies
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository
    (
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ILogger<AuthRepository> logger
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }
    #endregion

    /// <summary>
    /// Signs in the user if the credentials were valid, the operation happens using the Sign In Manager
    /// </summary>
    /// <param name="request">User credentials</param>
    /// <returns></returns>
    /// <exception cref="AuthFailedException"></exception>
    public async Task LoginUserAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email!);

        if (user is null)
            throw new AuthFailedException(message: "Invalid credentials");

        var correctPassword = await _userManager
            .CheckPasswordAsync(user, request.Password!);

        if (correctPassword)
        {
            _logger.LogInformation
            (
                "New user {username} with email {email} logged in successfully",
                user.UserName,
                user.Email
            );

            // sign in the user
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password!, false);
            return;
        }

        throw new AuthFailedException(message: "Email or password is incorrect");
    }

    public async Task RegisterUserAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email!);

        if (existingUser is not null)
            throw new UserCreationFailedException(message: "Something went wrong");

        var newUser = new AppUser()
        {
            FirstName = request.FirstName!,
            LastName = request.LastName!,
            DateOfBirth = request.DateOfBirth,
            UserName = request.Username,
            Email = request.Email,
            ProfilePictureUrl = request.ProfilePictureUrl
        };

        var userCreationResult = await _userManager.CreateAsync(newUser, request.Password!);

        // if the registration succeeded, sign in the user straight away
        if (userCreationResult.Succeeded)
        {
            _logger.LogInformation
            (
                "New user {username} with email {email} logged in successfully",
                newUser.UserName,
                newUser.Email
            );

            await _signInManager.SignInAsync(newUser, false, null);
            return;
        }

        throw new UserCreationFailedException(message: "Something went wrong while registering new account");
    }
}
