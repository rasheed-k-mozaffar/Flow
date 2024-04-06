using System.Security.Claims;
using BlazorAnimate;
using Flow.Client.Extensions;
using Flow.Client.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace Flow.Client.Components;

public partial class Conversation : ComponentBase
{
    private const int MAX_ALLOWED_FILE_SIZE = 1024 * 1024 * 10; // 10 Migs
    private static readonly string[] _allowedExtensions = { ".jpeg", ".png", ".webp", ".jpg" };

    private InputTextArea? messageInput;
    private SendMessageDto messageModel = new();

    [Parameter]
    public UserDetailsDto ContactModel { get; set; } = null!;

    [Parameter]
    public Guid ThreadId { get; set; }

    [Inject]
    public ApplicationState AppState { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Inject]
    public IJSRuntime Js { get; set; } = default!;

    [Inject]
    public IMessagesService MessagesService { get; set; } = default!;

    [Inject]
    public IFilesService FilesService { get; set; } = default!;

    [Inject]
    public IThreadsService ThreadsService { get; set; } = default!;

    private bool wantsToDeleteMessages = false;

    private ICollection<Guid> selectedMessages = new List<Guid>();
    private ICollection<IFormFile>? selectedImages;
    private MessageDto? selectedImageMessage = null;
    private string? threadId;
    private string _errorMessage = string.Empty;
    private bool _isMakingNetworkRequest = false;
    public AuthenticationState? authState;
    private ClaimsPrincipal currentUser = new();
    private string currentUserId = string.Empty;
    private bool isSendButtonEnabled = false;
    private bool isChatRendered = false;
    private bool wantsToTakePicture = false;
    private bool stillHasMessagesToLoad = true;

    private async Task ScrollToBottom(bool toBottom)
    {
        await Js.InvokeVoidAsync("scrollToBottom", "messages-area");
    }

    protected override async Task OnInitializedAsync()
    {
        authState = await AuthStateProvider.GetAuthenticationStateAsync();
        currentUser = authState.User;
        currentUserId = authState.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Js.InvokeVoidAsync("addScrollListener", DotNetObjectReference.Create(this), "messages-area");
        }

        if (!isChatRendered)
        {
            await Js.InvokeVoidAsync("scrollToBottom", "messages-area");
            isChatRendered = true;
        }

        try
        {
            if (messageInput?.Element != null && messageInput.Element.HasValue)
            {
                await messageInput.Element.Value.FocusAsync();
            }
        }
        catch (JSException) { }
    }

    protected override async Task OnParametersSetAsync()
    {
        isChatRendered = false;
        await base.OnParametersSetAsync();
        threadId = ThreadId.ToString();
        selectedMessages?.Clear();
        wantsToDeleteMessages = false;
    }

    private void SelectMessage(Guid messageId)
    {
        selectedMessages.Add(messageId);
    }

    private void UnSelectMessage(Guid messageId)
    {
        selectedMessages.Remove(messageId);
    }

    private void UpdateSendButtonVisibility(ChangeEventArgs eventArgs)
    {
        isSendButtonEnabled = !string.IsNullOrEmpty(eventArgs.Value?.ToString());
    }

    private async Task HandleSendingMessageAsync()
    {
        if (string.IsNullOrEmpty(messageModel.Content))
            return;

        messageModel.MessageId = Guid.NewGuid();
        messageModel.ThreadId = ThreadId!;
        messageModel.SenderId = currentUserId;
        if (AppState.ChatHubConnection is not null)
        {
            await AppState.ChatHubConnection.InvokeAsync("SendMessageAsync", messageModel);
            await ScrollToBottom(toBottom: true);

            AppState.NotifyStateChanged();

            messageModel = new();
            UpdateSendButtonVisibility(new ChangeEventArgs());
        }
    }

    [JSInvokable]
    public async Task HandleLoadingPreviousMessagesAsync()
    {
        if (!stillHasMessagesToLoad)
            return;

        _isMakingNetworkRequest = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            var lastMessage = AppState.Threads[ThreadId].Messages.First();

            var apiResponse = await ThreadsService.LoadPreviousMessagesAsync
            (
                new LoadPreviousMessagesRequest
                {
                    ThreadId = Guid.Parse(threadId!),
                    LastMessageDate = (DateTime)(lastMessage!.SentOn)
                }
            );

            if (apiResponse.IsSuccess)
            {
                AppState.Threads[ThreadId]
                        .Messages
                        .InsertRange(0, apiResponse.Body!.Messages);

                stillHasMessagesToLoad = apiResponse.Body.HasUnloadedMessages;
            }
        }
        catch (LoadingMessagesFailedException ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isMakingNetworkRequest = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task HandleSendingDeleteMessagesRequestAsync()
    {
        try
        {
            DeleteMessagesRequest request = new()
            {
                ThreadId = Guid.Parse(threadId!),
                MessagesIds = selectedMessages
            };

            await MessagesService.SendDeleteMessagesRequestAsync(request);
            selectedMessages.Clear();
        }
        catch (OperationFailureException ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            CloseConfirmMessageDeletesModal();
        }
    }

    private async Task HandleSendingImagesAsync()
    {
        _isMakingNetworkRequest = true;
        _errorMessage = string.Empty;
        try
        {
            if (selectedImages is not null && selectedImages.Any())
            {
                // perform the upload for each image
                foreach (var image in selectedImages)
                {
                    var apiResponse = await FilesService.UploadImageAsync(image, ImageType.NormalImage);

                    if (apiResponse.IsSuccess)
                    {
                        messageModel = new()
                        {
                            Type = MessageType.Image,
                            Content = apiResponse.Body!.RelativeUrl!
                        };

                        await HandleSendingMessageAsync();
                    }
                }
            }
            else
            {
                _errorMessage = "You didn't select any image";
                return;
            }
        }
        catch (FileUploadFailedException ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isMakingNetworkRequest = false;
            selectedImages?.Clear();
        }
    }

    private async Task GetSelectedImagesAsync(InputFileChangeEventArgs eventArgs)
    {
        _errorMessage = string.Empty;

        var files = eventArgs.GetMultipleFiles();
        selectedImages = new List<IFormFile>();

        foreach (var file in files)
        {
            if (file is not null)
            {
                var extension = Path.GetExtension(file.Name);

                if (!_allowedExtensions.Contains(extension))
                {
                    _errorMessage = "File format ({extension}) is not allowed";
                    continue;
                }

                IBrowserFile imageFile;

                if (!extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    imageFile = await file.RequestImageFileAsync(".jpeg", 1280, 720);
                }

                imageFile = file;


                if (imageFile.Size > MAX_ALLOWED_FILE_SIZE)
                {
                    _errorMessage = $"{file.Name}: File size is too large";
                    continue;
                }

                Console.WriteLine("Image Size: {0} Bytes", imageFile.Size);

                // read the file data
                var buffer = new byte[imageFile.Size];
                await imageFile.OpenReadStream(MAX_ALLOWED_FILE_SIZE).ReadAsync(buffer);

                // Convert to base64-encoded data URL
                var base64String = Convert.ToBase64String(buffer);
                var tempUrl = $"data:{imageFile.ContentType};base64,{base64String}";

                selectedImages.Add(FileConverter.ConvertToIFromFileFromBase64ImageString(tempUrl));
            }
        }

        await HandleSendingImagesAsync();
    }

    private async Task HandleSendingPictureAsync(string base64Image)
    {
        var formFile = FileConverter.ConvertToIFromFileFromBase64ImageString(base64Image);

        try
        {
            var apiResponse = await FilesService.UploadImageAsync(formFile, ImageType.NormalImage);

            if (apiResponse.IsSuccess)
            {
                messageModel = new()
                {
                    Type = MessageType.Image,
                    Content = apiResponse.Body!.RelativeUrl!
                };

                await HandleSendingMessageAsync();
            }
        }
        catch (FileUploadFailedException ex)
        {
            _errorMessage = ex.Message;
        }
    }

    private void OpenImageModal(MessageDto imageMessage)
    {
        selectedImageMessage = imageMessage;
    }

    private void OpenCameraView() => wantsToTakePicture = true;

    private void CloseCameraView() => wantsToTakePicture = false;

    private void OpenConfirmMessageDeletesModal() => wantsToDeleteMessages = true;

    private void CloseConfirmMessageDeletesModal() => wantsToDeleteMessages = false;
}
