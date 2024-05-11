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
    private UserDetailsDto? _contact;
    private const int MaxAllowedFileSize = 1024 * 1024 * 10; // 10 Migs
    private static readonly string[] AllowedExtensions = { ".jpeg", ".png", ".webp", ".jpg" };

    private InputText? _messageInput;
    private MessageDto _messageModel = new();
    private string? _messageContent;

    [Parameter]
    public ChatDetails ChatThread { get; set; } = null!;

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

    List<List<MessageDto>> _groupedMessages = new List<List<MessageDto>>();
    List<MessageDto> _currentGroup = new List<MessageDto>();

    private bool _wantsToDeleteMessages = false;
    private bool _wantsToViewChatDetails = false;

    private ICollection<Guid> _selectedMessages = new List<Guid>();
    private ICollection<IFormFile>? _selectedImages;
    private MessageDto? _selectedImageMessage = null;
    private string? _threadId;
    private string _errorMessage = string.Empty;
    private bool _isMakingNetworkRequest = false;
    private AuthenticationState? _authState;
    private ClaimsPrincipal _currentUser = new();
    private string _currentUserId = string.Empty;
    private bool _isSendButtonEnabled = false;
    private bool _isChatRendered = false;
    private bool _wantsToTakePicture = false;
    private bool _stillHasMessagesToLoad = true;

    private async Task ScrollToBottom(bool toBottom)
    {
        await Js.InvokeVoidAsync("scrollToBottom", "messages-area");
    }

    protected override async Task OnInitializedAsync()
    {
        _authState = await AuthStateProvider.GetAuthenticationStateAsync();
        _currentUser = _authState.User;
        _currentUserId = _authState.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
    }


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            await Js.InvokeVoidAsync("addScrollListener", DotNetObjectReference.Create(this), "messages-area");
        }

        if (!_isChatRendered)
        {
            await Js.InvokeVoidAsync("scrollToBottom", "messages-area");
            _isChatRendered = true;
        }

        try
        {
            if (_messageInput?.Element != null && _messageInput.Element.HasValue)
            {
                await _messageInput.Element.Value.FocusAsync();
            }
        }
        catch (JSException) { }
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (ChatThread.Type is ThreadType.Normal)
        {
            _contact = ChatThread.Participants.FirstOrDefault(p => p is not null && p.UserId != _currentUserId);
        }
        else {
            _contact = null;
        }
        _isChatRendered = false;
        _threadId = ThreadId.ToString();
        _selectedMessages?.Clear();
        _wantsToDeleteMessages = false;
        _wantsToViewChatDetails = false;
    }

    private void UpdateSendButtonVisibility(ChangeEventArgs eventArgs)
    {
        _isSendButtonEnabled = !string.IsNullOrEmpty(eventArgs.Value?.ToString());
    }

    private async Task HandleSendingMessageAsync()
    {
        if (string.IsNullOrEmpty(_messageContent))
            return;

        _messageModel.Content = new string(_messageContent);
        _messageContent = string.Empty;
        _messageModel.Id = Guid.NewGuid();
        _messageModel.ThreadId = ThreadId!;
        _messageModel.SenderId = _currentUserId;

        AppState.Threads[Guid.Parse(_threadId!)].Messages.Add(new MessageDto
        {
            Id = _messageModel.Id,
            ThreadId = _messageModel.ThreadId,
            Status = MessageStatus.Sending,

            Content = _messageModel.Content,
            SenderId = _currentUserId,
            SentOn = DateTime.UtcNow,
            Type = _messageModel.Type,
        });

        AppState.NotifyStateChanged();

        if (AppState.ChatHubConnection is not null)
        {
            await AppState.ChatHubConnection.InvokeAsync("SendMessageAsync", _messageModel);

            UpdateSendButtonVisibility(new ChangeEventArgs());
        }
    }

    [JSInvokable]
    public async Task HandleLoadingPreviousMessagesAsync()
    {
        if (!_stillHasMessagesToLoad)
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
                    ThreadId = Guid.Parse(_threadId!),
                    LastMessageDate = (DateTime)(lastMessage!.SentOn)
                }
            );

            if (apiResponse.IsSuccess)
            {
                AppState.Threads[ThreadId]
                        .Messages
                        .InsertRange(0, apiResponse.Body!.Messages);

                _stillHasMessagesToLoad = apiResponse.Body.HasUnloadedMessages;
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
                ThreadId = Guid.Parse(_threadId!),
                MessagesIds = _selectedMessages
            };

            await MessagesService.SendDeleteMessagesRequestAsync(request);
            _selectedMessages.Clear();
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
            if (_selectedImages is not null && _selectedImages.Any())
            {
                // perform the upload for each image
                foreach (var image in _selectedImages)
                {
                    var apiResponse = await FilesService.UploadImageAsync(image, ImageType.NormalImage);

                    if (apiResponse.IsSuccess)
                    {
                        _messageModel = new()
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
            _selectedImages?.Clear();
        }
    }

    private async Task GetSelectedImagesAsync(InputFileChangeEventArgs eventArgs)
    {
        _errorMessage = string.Empty;

        var files = eventArgs.GetMultipleFiles();
        _selectedImages = new List<IFormFile>();

        foreach (var file in files)
        {
            if (file is not null)
            {
                var extension = Path.GetExtension(file.Name);

                if (!AllowedExtensions.Contains(extension))
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


                if (imageFile.Size > MaxAllowedFileSize)
                {
                    _errorMessage = $"{file.Name}: File size is too large";
                    continue;
                }

                Console.WriteLine("Image Size: {0} Bytes", imageFile.Size);

                // read the file data
                var buffer = new byte[imageFile.Size];
                await imageFile.OpenReadStream(MaxAllowedFileSize).ReadAsync(buffer);

                // Convert to base64-encoded data URL
                var base64String = Convert.ToBase64String(buffer);
                var tempUrl = $"data:{imageFile.ContentType};base64,{base64String}";

                _selectedImages.Add(FileConverter.ConvertToIFromFileFromBase64ImageString(tempUrl));
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
                _messageModel = new()
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

    private void OpenChatDetailsModal()
    {
        _wantsToViewChatDetails = true;
    }

    private string? GetSenderName(string senderId)
    {
        if (senderId == AppState.CurrentUserId)
        {
            return null;
        }

        if (_contact is null)
        {
            return ChatThread.Participants.FirstOrDefault(p => p.UserId == senderId)!.Name;
        }
        else
        {
            return _contact.Name;
        }
    }

    private void SelectMessage(Guid messageId)
    {
        _selectedMessages.Add(messageId);
    }

    private void UnselectMessage(Guid messageId)
    {
        _selectedMessages.Remove(messageId);
    }

    private void OpenImageModal(MessageDto imageMessage)
    {
        _selectedImageMessage = imageMessage;
    }

    private void OpenCameraView() => _wantsToTakePicture = true;

    private void CloseCameraView() => _wantsToTakePicture = false;

    private void OpenConfirmMessageDeletesModal() => _wantsToDeleteMessages = true;

    private void CloseConfirmMessageDeletesModal() => _wantsToDeleteMessages = false;
}
