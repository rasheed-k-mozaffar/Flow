using BlazorAnimate;
using Flow.Client.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace Flow.Client.Pages.Auth;

public partial class SignUp : ComponentBase
{
    private const int MAX_ALLOWED_FILE_SIZE = 1024 * 1024 * 5; // 5 Migs
    private static readonly string[] _allowedExtensions = { ".jpeg", ".png", ".webp", ".jpg" };
    private bool isMakingRequest = false;
    private string errorMessage = string.Empty;

    private Animate? firstFormAnimation;
    private Animate? secondFormFadeIn;
    private Animate? finalFormAnimation;
    bool firstFormValid = false;

    private bool displayFirstForm = true;
    private bool displaySecondForm = false;
    private bool displayFinalForm = false;
    private bool wantsToCaptureProfilePicture = false;

    protected EditContext? EC { get; set; }
    private IFormFile? profilePictureFile;
    private string? profilePictureTempUrl;

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

    [Inject]
    public IFilesService FilesService { get; set; } = default!;

    #endregion

    protected override void OnInitialized()
    {
        EC = new EditContext(requestModel);
        //EC.OnFieldChanged += EditContext_OnFieldChanged!;
        firstFormAnimation?.Run();
        base.OnInitialized();
    }

    private void MoveToFinalForm()
    {
        displayFinalForm = true;
        finalFormAnimation?.Run();
        displaySecondForm = false;
    }

    private async Task HandleUserRegistrationAsync()
    {
        InitStateVariables();

        try
        {
            var registrationResult = await AuthService.RegisterUserAsync(requestModel);

            if (registrationResult.IsSuccess)
            {
                await JwtsManager.SetJwtAsync(registrationResult.Body!, true);

                await AuthenticationStateProvider.GetAuthenticationStateAsync();

                if (profilePictureFile is not null)
                {
                    await UploadProfilePictureAsync();
                }

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

    private async Task UploadProfilePictureAsync()
    {
        try
        {
            var apiResponse = await FilesService
                .UploadImageAsync(profilePictureFile!, ImageType.ProfilePicture);

            if (apiResponse.IsSuccess)
            {
                requestModel.ProfilePictureUrl = apiResponse.Body!.RelativeUrl;
            }
        }
        catch (FileUploadFailedException ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task SelectProfilePicture(InputFileChangeEventArgs eventArgs)
    {
        errorMessage = string.Empty;
        var file = eventArgs.File;

        if (file is not null) // validate and process the image
        {
            var extension = Path.GetExtension(file.Name);

            if (!_allowedExtensions.Contains(extension))
            {
                errorMessage = "File format is not allowed";
                return;
            }

            IBrowserFile imageFile;

            if (!extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                imageFile = await eventArgs.File.RequestImageFileAsync(".jpeg", 500, 500);
            }

            imageFile = eventArgs.File;


            if (imageFile.Size > MAX_ALLOWED_FILE_SIZE)
            {
                errorMessage = "File size is too large";
                return;
            }

            Console.WriteLine("Image Size: {0} Bytes", imageFile.Size);

            // read the file data
            var buffer = new byte[file.Size];
            await imageFile.OpenReadStream(MAX_ALLOWED_FILE_SIZE).ReadAsync(buffer);

            // Convert to base64-encoded data URL
            var base64String = Convert.ToBase64String(buffer);
            profilePictureTempUrl = $"data:{imageFile.ContentType};base64,{base64String}";
            profilePictureFile = FileConverter.ConvertToIFromFileFromBase64ImageString(profilePictureTempUrl);
        }
    }

    private void OpenCameraModal() => wantsToCaptureProfilePicture = true;
    private void CloseCameraModal() => wantsToCaptureProfilePicture = false;
    private void GetCapturedImageUri(string capturedFrame) => profilePictureTempUrl = capturedFrame;


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
        var validate = EC!.Validate();
        var numberOfWrongFields = EC.GetValidationMessages();
        if (numberOfWrongFields.Count() == 3)
        {
            firstFormValid = true;
        }
        if (firstFormValid)
        {
            displaySecondForm = true;
            secondFormFadeIn?.Run();
            displayFirstForm = false;
        }
    }

    private void GoBackToFirstForm()
    {
        displayFirstForm = true;
        firstFormAnimation?.Run();
        displaySecondForm = false;
        displayFinalForm = false;
    }

    private void GoBackToSecondForm()
    {
        displaySecondForm = true;
        secondFormFadeIn?.Run();
        displayFinalForm = false;
    }
}
