using Flow.Shared.ApiResponses;
using System.Net.Http.Json;

namespace Flow.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<ApiResponse<string>> RegisterUserAsync(RegisterRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync< RegisterRequest>("/api/auth/register", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                throw new AuthFailedException(message: error!.ErrorMessage!, errors: error.Errors);
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result!;
        }
        public async Task<ApiResponse<string>> LoginUserAsync(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync< LoginRequest>("/api/auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                throw new AuthFailedException(message: error!.ErrorMessage!);
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result!;
        }
    }
}
