using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Flow.Client;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJwtsManager _jwtsManager;

    public JwtAuthenticationStateProvider(IJwtsManager jwtsManager)
    {
        _jwtsManager = jwtsManager;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // check the browser's storage for a JWT
        if (await _jwtsManager.CheckForJwtAsync())
        {
            string? tokenString = await _jwtsManager.GetJwtAsync();
            JwtSecurityToken jwt = new JwtSecurityTokenHandler().ReadJwtToken(tokenString);

            // Create the claims identity using the claims in the JWT and use Bearer as the auth type
            ClaimsIdentity identity = new ClaimsIdentity(jwt.Claims, "Bearer");
            ClaimsPrincipal user = new ClaimsPrincipal(identity);

            var authenticationState = new AuthenticationState(user);

            // Raise the event to notify the authentication state has changed
            NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));

            return authenticationState;
        }

        // give the user an anonymous identity
        ClaimsPrincipal anonymousUser = new ClaimsPrincipal();
        return new AuthenticationState(anonymousUser);
    }
}
