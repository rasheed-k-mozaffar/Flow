using Microsoft.AspNetCore.Components;

namespace Flow.Client.Pages.Auth;

public partial class SignIn : ComponentBase
{
    private bool isMakingRequest = false;
    private string errorMessage = string.Empty;

    private LoginRequest requestModel = new();

    #region Dependencies
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    #endregion

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
