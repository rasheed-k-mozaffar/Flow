namespace Flow.Server.Repositories;

public interface IAuthRepository
{
    Task<UserManagerResponse> LoginUserAsync(LoginRequest request);
    Task<UserManagerResponse> RegisterUserAsync(RegisterRequest request);
}