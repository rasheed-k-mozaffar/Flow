using BlazorAnimate;
using Flow.Client.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace Flow.Client.Pages.Auth;

public partial class SignUp : ComponentBase
{
    private const int MaxAllowedFileSize = 1024 * 1024 * 5; // 5 Migs
    private static readonly string[] AllowedExtensions = { ".jpeg", ".png", ".webp", ".jpg" };
    private bool _isMakingRequest = false;
    private string _errorMessage = string.Empty;
    private List<string>? _errors;

    private Animate? _firstFormAnimation;
    private Animate? _secondFormFadeIn;
    private Animate? _finalFormAnimation;
    private bool _firstFormValid = false;

    private bool _displayFirstForm = true;
    private bool _displaySecondForm = false;
    private bool _displayFinalForm = false;
    private bool _wantsToCaptureProfilePicture = false;

    private EditContext? Ec { get; set; }
    private IFormFile? _profilePictureFile;
    private string? _profilePictureTempUrl;

    private readonly RegisterRequest _requestModel = new();

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
        Ec = new EditContext(_requestModel);
        _firstFormAnimation?.Run();
        base.OnInitialized();
    }

    private void MoveToFinalForm()
    {
        _displayFinalForm = true;
        _finalFormAnimation?.Run();
        _displaySecondForm = false;
    }

    private async Task HandleUserRegistrationAsync()
    {
        InitStateVariables();

        try
        {
            var registrationResult = await AuthService.RegisterUserAsync(_requestModel);

            if (registrationResult.IsSuccess)
            {
                await JwtsManager.SetJwtAsync(registrationResult.Body!, true);

                await AuthenticationStateProvider.GetAuthenticationStateAsync();

                if (_profilePictureFile is not null)
                {
                    await UploadProfilePictureAsync();
                }

                Nav.NavigateTo("/");
            }
        }
        catch (AuthFailedException ex)
        {
            _errorMessage = ex.Message;
            _errors = ex.Errors;
        }
        finally
        {
            _isMakingRequest = false;
        }
    }

    private async Task UploadProfilePictureAsync()
    {
        try
        {
            var apiResponse = await FilesService
                .UploadImageAsync(_profilePictureFile!, ImageType.ProfilePicture);

            if (apiResponse.IsSuccess)
            {
                _requestModel.ProfilePictureUrl = apiResponse.Body!.RelativeUrl;
            }
        }
        catch (FileUploadFailedException ex)
        {
            _errorMessage = ex.Message;
        }
    }

    private async Task SelectProfilePicture(InputFileChangeEventArgs eventArgs)
    {
        _errorMessage = string.Empty;
        var file = eventArgs.File;

        if (file is not null) // validate and process the image
        {
            var extension = Path.GetExtension(file.Name);

            if (!AllowedExtensions.Contains(extension))
            {
                _errorMessage = "File format is not allowed";
                return;
            }

            IBrowserFile imageFile;

            if (!extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                imageFile = await eventArgs.File.RequestImageFileAsync(".jpeg", 500, 500);
            }

            imageFile = eventArgs.File;


            if (imageFile.Size > MaxAllowedFileSize)
            {
                _errorMessage = "File size is too large";
                return;
            }

            Console.WriteLine("Image Size: {0} Bytes", imageFile.Size);

            // read the file data
            var buffer = new byte[file.Size];
            await imageFile.OpenReadStream(MaxAllowedFileSize).ReadAsync(buffer);

            // Convert to base64-encoded data URL
            var base64String = Convert.ToBase64String(buffer);
            _profilePictureTempUrl = $"data:{imageFile.ContentType};base64,{base64String}";
            _profilePictureFile = FileConverter.ConvertToIFromFileFromBase64ImageString(_profilePictureTempUrl);
        }
    }

    private void OpenCameraModal() => _wantsToCaptureProfilePicture = true;
    private void CloseCameraModal() => _wantsToCaptureProfilePicture = false;
    private void GetCapturedImageUri(string capturedFrame) => _profilePictureTempUrl = capturedFrame;


    /// <summary>
    /// Gets called when the close button on the error alert
    /// </summary>
    private void CloseErrorAlert() => _errorMessage = string.Empty;

    /// <summary>
    /// Sets isMakingRequest to true to disable form buttons
    /// Sets errorMessage to empty again so that if an error was already there, it would reset it
    /// </summary>
    private void InitStateVariables()
    {
        _isMakingRequest = true;
        _errorMessage = string.Empty;
    }

    private void EditContext_OnFieldChanged(object sender, FieldChangedEventArgs e)
    {
        var validate = Ec!.Validate();
        var numberOfWrongFields = Ec.GetValidationMessages();
        if (numberOfWrongFields.Count() == 3)
        {
            _firstFormValid = true;
        }
    }
    private void ValidateAndPlayAnimation()
    {
        var validate = Ec!.Validate();
        var numberOfWrongFields = Ec.GetValidationMessages();
        if (numberOfWrongFields.Count() == 3)
        {
            _firstFormValid = true;
        }
        if (_firstFormValid)
        {
            _displaySecondForm = true;
            _secondFormFadeIn?.Run();
            _displayFirstForm = false;
        }
    }

    private void GoBackToFirstForm()
    {
        _displayFirstForm = true;
        _firstFormAnimation?.Run();
        _displaySecondForm = false;
        _displayFinalForm = false;
    }

    private void GoBackToSecondForm()
    {
        _displaySecondForm = true;
        _secondFormFadeIn?.Run();
        _displayFinalForm = false;
    }
}
