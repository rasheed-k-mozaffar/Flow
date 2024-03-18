using System.Runtime.InteropServices;
using Flow.Client.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Flow.Client.Pages;

public partial class Index : ComponentBase, IAsyncDisposable
{
    private const string API_URL = "https://localhost:7292/chat-threads-hub";

    [Inject]
    public ApplicationState AppState { get; set; } = default!;

    [Inject]
    public IJwtsManager JwtsManager { get; set; } = default!;

    [Inject]
    public IThreadsService ThreadsService { get; set; } = default!;

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        AppState.OnChange += StateHasChanged;
        var userJwt = await JwtsManager.GetJwtAsync();
        AppState.UserJwt = userJwt;

        AppState.AuthState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        await LoadThreadsAndMessagesAsync();
        await AppState.InitializeHubsAsync();

    }

    public async ValueTask DisposeAsync()
    {
        AppState.OnChange -= StateHasChanged;

        if (AppState.ChatHubConnection is not null)
        {
            await AppState.ChatHubConnection.DisposeAsync();
        }

        if (AppState.ContactsHubConnection is not null)
        {
            await AppState.ContactsHubConnection.DisposeAsync();
        }
    }

    public async Task LoadThreadsAndMessagesAsync()
    {
        var apiResponse = await ThreadsService.GetPreliminaryThreadsDetailsForUserAsync();

        if (apiResponse.IsSuccess)
        {
            AppState.Threads = apiResponse.Body;
        }
    }
}
