using Flow.Client.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace Flow.Client.Components;

public partial class CreateGroupChatModal : ComponentBase
{
    private const int MaxAllowedFileSize = 1024 * 1024 * 5; // 5 Migs
    private static readonly string[] AllowedExtensions = { ".jpeg", ".png", ".webp", ".jpg" };

    [Inject] public IGroupsService GroupsService { get; set; } = default!;

    [Inject] public IFilesService FilesService { get; set; } = default!;

    [Parameter] public bool Show { get; set; }
    [Parameter] public EventCallback OnCloseClicked { get; set; }

    private CreateGroupRequest _request = new();
    private List<UserDetailsDto> _userContacts = new();
    private List<UserDetailsDto> _filteredContacts = new();
    private HashSet<string> _selectedContacts = new();

    private string _groupPictureTempUrl = string.Empty;
    private IFormFile? _groupPictureFile;
    private string? _searchTerm = null;
    private string _errorMessage = string.Empty;
    private bool _isMakingApiCall = false;

    protected override async Task OnParametersSetAsync()
    {
        _userContacts = AppState.Threads.Values
            .Where(thread => thread.Type == ThreadType.Normal)
            .SelectMany(thread => thread.Participants)
            .Distinct()
            .Where(user => user.UserId != AppState.CurrentUserId)
            .OrderBy(user => user.Name)
            .ToList();

        _filteredContacts = _userContacts;

        await base.OnParametersSetAsync();
    }

    private async Task HandleCreatingGroupAsync()
    {
        try
        {
            _isMakingApiCall = true;
            _errorMessage = string.Empty;

            if (_selectedContacts.Count < 1)
            {
                _errorMessage = "You must select at least one contact";
                return;
            }

            _request.Participants = _selectedContacts.ToList();
            _request.Participants.Add(AppState.CurrentUserId!);
            var apiResponse = await GroupsService.CreateGroupAsync(_request);

            if (apiResponse.IsSuccess)
            {
                if (_groupPictureFile is not null)
                {
                    await UploadGroupPictureAsync();
                }

                Show = false;
            }
        }
        catch (OperationFailureException ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isMakingApiCall = false;
        }
    }

    private void OnSearchValueChanged(ChangeEventArgs args)
    {
        _searchTerm = args.Value?.ToString();

        if (!string.IsNullOrEmpty(_searchTerm))
        {
            _filteredContacts = _userContacts
                .Where(user => user.Name!.ToLower().Contains(_searchTerm.ToLower()))
                .ToList();
        }
        else
        {
            _filteredContacts = _userContacts;
        }
    }

    private void CloseButtonClicked()
    {
        OnCloseClicked.InvokeAsync();
    }

    private void CloseErrorAlert() => _errorMessage = string.Empty;

    private void OnContactClicked(string selectedUser)
    {
        if (!_selectedContacts.Add(selectedUser))
        {
            _selectedContacts.Remove(selectedUser);
        }

        StateHasChanged();
    }

    private async Task UploadGroupPictureAsync()
    {
        _errorMessage = string.Empty;
        _isMakingApiCall = true;

        try
        {
            var apiResponse = await FilesService
                                .UploadImageAsync
                                    (
                                        _groupPictureFile!,
                                        ImageType.GroupPicture,
                                        _request.GroupThreadId
                                    );

            if (apiResponse.IsSuccess)
            {
                _request.GroupPictureUrl = apiResponse.Body!.RelativeUrl;
            }
        }
        catch (FileUploadFailedException ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isMakingApiCall = false;
        }
    }

    private async Task SelectGroupPicture(InputFileChangeEventArgs args)
    {
        _errorMessage = string.Empty;
        var file = args.File;

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
                imageFile = await args.File.RequestImageFileAsync(".jpeg", 500, 500);
            }

            imageFile = args.File;


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
            _groupPictureTempUrl = $"data:{imageFile.ContentType};base64,{base64String}";
            _groupPictureFile = FileConverter.ConvertToIFromFileFromBase64ImageString(_groupPictureTempUrl);
        }
    }

    private void RemoveGroupPicture()
    {
        _groupPictureFile = null;
        _groupPictureTempUrl = string.Empty;
    }
}