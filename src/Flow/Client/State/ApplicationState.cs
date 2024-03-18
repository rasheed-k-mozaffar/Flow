using System.Security.Claims;
using Flow.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace Flow.Client.State;

public class ApplicationState
{
    public const string CHAT_HUB_URL = "https://localhost:7292/chat-threads-hub";
    public const string CONTACTS_HUB_URL = "https://localhost:7292/contacts-hub";

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
    public ICollection<ContactDto> Contacts { get; set; }

    public List<PendingRequestIncomingDto> IncomingContactRequests { get; set; }
    public List<PendingRequestSentDto> SentContactRequests { get; set; }

    public HubConnection ChatHubConnection { get; private set; } = default!;
    public HubConnection ContactsHubConnection { get; private set; } = default!;

    public AuthenticationState AuthState { get; set; } = default!;

    public string? CurrentUserId { get; set; }

    public string? UserJwt { get; set; }

    public ApplicationState(IJSRuntime js)
    {
        _js = js;
        IncomingContactRequests = new();
        SentContactRequests = new();
        Contacts = new List<ContactDto>();
    }

    public void NotifyStateChanged() => OnChange?.Invoke();

    public async Task InitializeHubsAsync()
    {
        var chatHubInitTask = InitChatHubConnection();
        var contactsHubInitTask = InitContactsHubConnection();

        await Task.WhenAll(chatHubInitTask, contactsHubInitTask);
    }

    private async Task InitChatHubConnection()
    {
        CurrentUserId = AuthState.User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        ChatHubConnection = new HubConnectionBuilder()
        .WithUrl(CHAT_HUB_URL, o =>
        {
            o.AccessTokenProvider = () => Task.FromResult<string?>(UserJwt);
        })
        .Build();


        ChatHubConnection.On<MessageDto>("ReceiveMessageAsync", async message =>
        {
            Threads?[message.ThreadId.ToString()].Add(message);
            await _js.InvokeVoidAsync("playMessageSound", message.SenderId == CurrentUserId);
            NotifyStateChanged();
        });

        ChatHubConnection.On<DeleteMessagesRequest>("ReceiveDeletedMessagesIdsAsync", request =>
        {
            Threads?[request.ThreadId.ToString()].RemoveAll(m => request.MessagesIds!.Contains(m.Id));
            NotifyStateChanged();
        });

        await ChatHubConnection.StartAsync();
    }

    private async Task InitContactsHubConnection()
    {
        ContactsHubConnection = new HubConnectionBuilder()
        .WithUrl(CONTACTS_HUB_URL, o =>
        {
            o.AccessTokenProvider = () => Task.FromResult<string?>(UserJwt);
        }).Build();

        ContactsHubConnection.On<PendingRequestIncomingDto>("ReceiveContactRequestAsync", async incomingRequest =>
        {
            IncomingContactRequests.Add(incomingRequest);
            await _js.InvokeVoidAsync("playRequestSound");
            NotifyStateChanged();
        });

        ContactsHubConnection.On<PendingRequestSentDto>("ReceiveSentContactRequestAsync", sentRequest =>
        {
            SentContactRequests.Add(sentRequest);
            NotifyStateChanged();
        });

        ContactsHubConnection.On<Guid>("ReceiveCancelledRequestIdForRecipientAsync", cancelledRequestId =>
        {
            var cancelledRequest = IncomingContactRequests
                                    .First(r => r.RequestId == cancelledRequestId);
            IncomingContactRequests.Remove(cancelledRequest);
            NotifyStateChanged();
        });

        ContactsHubConnection.On<Guid>("ReceiveCancelledRequestIdForSenderAsync", cancelledRequestId =>
        {
            var cancelledRequest = SentContactRequests
                                    .First(r => r.RequestId == cancelledRequestId);
            SentContactRequests.Remove(cancelledRequest);
            NotifyStateChanged();
        });

        ContactsHubConnection.On<ContactDto>("ReceiveNewContact", async newContact =>
        {
            Contacts.Add(newContact);
            Threads?.Add(newContact.ThreadId.ToString()!, new());

            // * Join the participants to the Hub groupd
            await ChatHubConnection.InvokeAsync("JoinThreadAsync", newContact.ThreadId);

            NotifyStateChanged();
        });

        ContactsHubConnection.On<Guid>("ReceiveAcceptedRequestId", acceptedRequestId =>
        {
            var acceptedRequest = SentContactRequests
                                    .First(r => r.RequestId == acceptedRequestId);

            SentContactRequests.Remove(acceptedRequest);
            NotifyStateChanged();
        });

        await ContactsHubConnection.StartAsync();
    }
}
