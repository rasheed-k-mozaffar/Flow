using Blazored.LocalStorage;
using Blazored.SessionStorage;

namespace Flow.Client.Services;

public class JwtsManager : IJwtsManager
{
    private readonly string _accessTokenKey;
    private readonly ILocalStorageService _localStorage;
    private readonly ISessionStorageService _sessionStorage;
    private readonly IConfiguration _configuration;

    public JwtsManager
    (
        ILocalStorageService localStorage,
        IConfiguration configuration,
        ISessionStorageService sessionStorage
    )
    {
        _localStorage = localStorage;
        _configuration = configuration;
        _sessionStorage = sessionStorage;
        _accessTokenKey = _configuration["AuthSettings:AccessTokensKey"]!;
    }

    public async Task<bool> CheckForJwtAsync()
    {
        return await _localStorage.ContainKeyAsync(_accessTokenKey) ||
               await _sessionStorage.ContainKeyAsync(_accessTokenKey);

    }

    public async Task<string?> GetJwtAsync()
    {
        if (await _localStorage.ContainKeyAsync(_accessTokenKey))
        {
            var token = await _localStorage
                .GetItemAsStringAsync(_accessTokenKey);

            return token;
        }
        else if (await _sessionStorage.ContainKeyAsync(_accessTokenKey))
        {
            var token = await _sessionStorage
                .GetItemAsStringAsync(_accessTokenKey);

            return token;
        }
        else
        {
            return null;
        }
    }

    public async Task SetJwtAsync(string token, bool isPersistent)
    {
        if (isPersistent) // * set token in local storage
            await _localStorage.SetItemAsStringAsync(_accessTokenKey, token);

        else // * set token in session storage
            await _sessionStorage.SetItemAsStringAsync(_accessTokenKey, token);
    }
}
