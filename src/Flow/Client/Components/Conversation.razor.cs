using System.Security.Claims;
using Flow.Client.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace Flow.Client.Components;

public partial class Conversation : ComponentBase
{
    private SendMessageDto messageModel = new();

    [Parameter] public ContactDto ContactModel { get; set; } = null!;

    [Inject]
    public ApplicationState AppState { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    [Inject]
    public IJSRuntime Js { get; set; } = default!;

    [Inject]
    public IMessagesService MessagesService { get; set; } = default!;

    private bool wantsToDeleteMessages = false;

    private ICollection<Guid> selectedMessages = new List<Guid>();
    private string? threadId;
    private string _errorMessage = string.Empty;
    public AuthenticationState? authState;
    private ClaimsPrincipal currentUser = new();

    protected override async Task OnInitializedAsync()
    {
        // * subscribe to the OnChange event that gets fired when a new message is received
        AppState.OnChange += StateHasChanged;
        authState = await AuthStateProvider.GetAuthenticationStateAsync();
        currentUser = authState.User;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await Js.InvokeVoidAsync("scrollToBottom", "messages-area");
    }

    protected override async Task OnParametersSetAsync()
    {
        await Js.InvokeVoidAsync("scrollToBottom", "messages-area");
        await base.OnParametersSetAsync();
        threadId = ContactModel.ThreadId.ToString();
        selectedMessages.Clear();
        wantsToDeleteMessages = false;

    }

    private async Task HandleSendingMessageAsync()
    {
        messageModel.ThreadId = (Guid)ContactModel.ThreadId!;
        messageModel.SenderId = currentUser.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        if (AppState.HubConnection is not null)
        {
            await AppState.HubConnection.InvokeAsync("SendMessageAsync", messageModel);
            await Js.InvokeVoidAsync("scrollToBottom", "messages-area");
            AppState.NotifyStateChanged();
            messageModel = new();
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

    private void OpenConfirmMessageDeletesModal() => wantsToDeleteMessages = true;

    private void CloseConfirmMessageDeletesModal() => wantsToDeleteMessages = false;
}
