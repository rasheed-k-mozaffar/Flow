using Flow.Client.State;
using Microsoft.AspNetCore.Components;

namespace Flow.Client.Components;

public partial class ChatDetailsModal : ComponentBase
{
    [Inject] public ApplicationState AppState { get; set; } = default!;
    [Inject] public IThreadsService ThreadsService { get; set; } = default!;
    [Parameter] public ChatDetails Chat { get; set; } = null!;
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback<MessageDto> OnImageClicked { get; set; }
    [Parameter] public bool Show { get; set; }

    private CancellationTokenSource _cts = new CancellationTokenSource();
    private UserDetailsDto? _contact; // used in case the chat type is Normal
    private List<MessageDto>? _media;

    private LoadChatMediaRequest? _loadMediaRequest;

    private string _errorMessage = string.Empty;
    private bool _isMakingApiCall = false;
    private bool _displayLoadMoreButton = false;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _loadMediaRequest = new LoadChatMediaRequest()
        {
            ChatThreadId = Chat.ChatThreadId,
            LoadNumber = 0,
            LoadSize = 2
        };
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (Chat.Type is ThreadType.Normal)
        {
            _contact = Chat
                .Participants
                .First(p => p.UserId != AppState.CurrentUserId);
        }
        else
        {
            _contact = null;
        }
    }

    private async Task LoadChatMedia()
    {
        _errorMessage = string.Empty;
        _isMakingApiCall = true;
        try
        {
            var apiResponse = await ThreadsService
                .GetChatMediaAsync(
                    _loadMediaRequest!,
                    _cts.Token
                );

            if (apiResponse.IsSuccess)
            {
                if (_media is null)
                {
                    _media = new List<MessageDto>();
                }

                _media.AddRange(apiResponse.Body!.Media);
                _displayLoadMoreButton = !(apiResponse.Body!.HasLoadedAll);
                _loadMediaRequest!.LoadNumber++;
                StateHasChanged();
            }
        }
        catch (ApiGetRequestFailedException ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isMakingApiCall = false;
        }
    }

    private void ImageClicked(MessageDto imgMessage)
    {
        OnImageClicked.InvokeAsync(imgMessage);
    }

    private void CloseModal()
    {
        Show = false;
    }
}