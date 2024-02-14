using BlazorAnimate;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;

namespace Flow.Client.Pages.Auth;

public partial class SignUp : ComponentBase
{
    private bool isMakingRequest = false;
    private string errorMessage = string.Empty;

    private Animate? secondFormFadeIn;
    bool firstFormValid = false;
    private bool secondFormTime = false;

    protected EditContext? EC { get; set; }

    private RegisterRequest requestModel = new();

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

    protected override void OnInitialized()
    {
        EC = new EditContext(requestModel);
        EC.OnFieldChanged += EditContext_OnFieldChanged!;
        base.OnInitialized();
    }

    private async Task HandleUserRegistrationAsync()
    {
        InitStateVariables();

        try
        {
            var apiResponse = await AuthService.RegisterUserAsync(requestModel);

            if (apiResponse.IsSuccess)
            {
                string token = apiResponse.Body!;
                await JwtsManager.SetJwtAsync(token, false);

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
    /// Gets called when the close button on the error alert
    /// </summary>
    private void CloseErrorAlert() => errorMessage = string.Empty;

    /// <summary>
    /// Sets isMakingRequest to true to disable form buttons
    /// Sets errorMessage to empty again so that if an error was already there, it would reset it
    /// </summary>
    private void InitStateVariables()
    {
        isMakingRequest = true;
        errorMessage = string.Empty;
    }

    private void EditContext_OnFieldChanged(object sender, FieldChangedEventArgs e)
    {
        var validate = EC!.Validate();
        var numberOfWrongFields = EC.GetValidationMessages();
        if (numberOfWrongFields.Count() == 3)
        {
            firstFormValid = true;
        }
    }
    private void ValidateAndPlayAnimation()
    {
        if (firstFormValid)
        {
            secondFormFadeIn?.Run();
            secondFormTime = true;
        }
    }
}
