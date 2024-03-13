using System.Security.Claims;
using Flow.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace Flow.Client.State;

public class ApplicationState
{
    public const string API_URL = "https://localhost:7292/chat-threads-hub";

    private readonly IJSRuntime _js;

    public event Action? OnChange;
    /// <summary>
    /// This represents the chat thread id (Contact) the user is currently in
    /// </summary>
    public ContactDto? SelectedThread { get; set; }

    /// <summary>
    /// This dictionary will store the ids of each thread the user is part of
    /// and the last 15 messages sent in each thread, this will be used when the app first loads to populate the contacts and t
    /// </summary>
    public Dictionary<string, List<MessageDto>>? Threads { get; set; }

    public HubConnection HubConnection { get; private set; } = default!;

    public AuthenticationState AuthState { get; set; } = default!;

    public string? CurrentUserId { get; set; }

    public string? UserJwt { get; set; }

    public ApplicationState(IJSRuntime js)
    {
        _js = js;
    }

    public void NotifyStateChanged() => OnChange?.Invoke();

    public async Task InitHubConnection()
    {
        CurrentUserId = AuthState.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        HubConnection = new HubConnectionBuilder()
        .WithUrl(API_URL, o =>
        {
            o.AccessTokenProvider = () => Task.FromResult<string?>(UserJwt);
        })
        .Build();


        HubConnection.On<MessageDto>("ReceiveMessageAsync", async message =>
        {
            Threads?[message.ThreadId.ToString()].Add(message);
            await _js.InvokeVoidAsync("playMessageSound", message.SenderId == CurrentUserId);
            NotifyStateChanged();
        });

        HubConnection.On<DeleteMessagesRequest>("ReceiveDeletedMessagesIdsAsync", request =>
        {
            Threads?[request.ThreadId.ToString()].RemoveAll(m => request.MessagesIds!.Contains(m.Id));
            NotifyStateChanged();
        });

        await HubConnection.StartAsync();
    }
}
