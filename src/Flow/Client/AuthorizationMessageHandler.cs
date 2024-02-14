namespace Flow.Client;

public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly IJwtsManager _jwtsManager;

    public AuthorizationMessageHandler(IJwtsManager jwtsManager)
    {
        _jwtsManager = jwtsManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (await _jwtsManager.CheckForJwtAsync())
        {
            string? tokenString = await _jwtsManager.GetJwtAsync();

            // Set the authorization header to use Bearer and write the token in it
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenString);

        }

        return await base.SendAsync(request, cancellationToken);
    }
}
