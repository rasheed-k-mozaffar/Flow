using Flow.Shared.ApiResponses;

namespace Flow.Client.Services
{
    public interface IAuthService
    {
        public Task<ApiResponse<string>> RegisterUserAsync(RegisterRequest request);
        public Task<ApiResponse<string>> LoginUserAsync(LoginRequest request)

    }
}
