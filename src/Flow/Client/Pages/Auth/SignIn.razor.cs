using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Flow.Client.Pages.Auth;

public partial class SignIn : ComponentBase
{
    private bool isMakingRequest = false;
    private string errorMessage = string.Empty;

    private LoginRequest requestModel = new();

    #region Dependencies
    [Inject]
    public NavigationManager Nav { get; set; } = default!;

    [Inject]
    public IAuthService AuthService { get; set; } = default!;

    [Inject]
    public IJwtsManager JwtsManager { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    #endregion


    private async Task HandleUserLoginAsync()
    {
        InitStateVariables();

        try
        {
            var apiResponse = await AuthService.LoginUserAsync(requestModel);

            if (apiResponse.IsSuccess)
            {
                string token = apiResponse.Body!;
                await JwtsManager.SetJwtAsync(token, requestModel.IsPersistent);

                await AuthenticationStateProvider.GetAuthenticationStateAsync();
                Nav.NavigateTo("/");
            }
        }
        catch (AuthFailedException ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isMakingRequest = false;
        }
    }

    /// <summary>
    /// Sets isMakingRequest to true to disable form buttons
    /// Sets errorMessage to empty again so that if an error was already there, it would reset it
    /// </summary>
    private void InitStateVariables()
    {
        isMakingRequest = true;
        errorMessage = string.Empty;
    }
}
