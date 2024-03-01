using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Windows.Markup;
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

    private string? threadId;
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
}
