namespace Flow.Repositories;

public interface IAuthRepository
{
    Task LoginUserAsync(LoginRequest request);
    Task RegisterUserAsync(RegisterRequest request);
}
