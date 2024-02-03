using BlazorAnimate;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Flow.Client.Pages.Auth;

public partial class SignUp : ComponentBase
{
#pragma warning disable CS0414
    private bool isMakingRequest = false;
    private string errorMessage = string.Empty;

    private Animate? secondFormFadeIn;
    bool firstFormValid = false;
    private bool secondFormTime = false;

    protected EditContext? EC { get; set; }

    private RegisterRequest requestModel = new();

    #region Dependencies
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    public IAuthService AuthService { get; set; } = default!;

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
            var result = await AuthService.RegisterUserAsync(requestModel);

            if (result.IsSuccess)
            {
                // TODO: Add the token to the local storage & navigate the user
                Console.WriteLine($"JWT: {result.Body?.Substring(0, 25)}");
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
