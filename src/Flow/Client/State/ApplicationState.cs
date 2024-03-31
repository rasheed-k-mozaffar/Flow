using System.Security.Claims;
using Flow.Client.Enums;
using Flow.Client.Settings;
using Flow.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;

namespace Flow.Client.State;

public class ApplicationState
{
    public readonly string _chatHubUrl = "https://localhost:7292/chat-threads-hub";
    public readonly string _contactsHubUrl = "https://localhost:7292/contacts-hub";

    private readonly IJSRuntime _js;
    private readonly AppSettings _appSettings;

    /// <summary>
    /// Represents the user's current settings and color scheme
    /// </summary>
    public UserSettingsDto UserSettings { get; set; }

    public SettingsItemName? SelectedSettings { get; set; } = null;

    public event Action? OnChange;
    /// <summary>
    /// This represents the chat thread id (Contact) the user is currently in
    /// </summary>

    public Guid SelectedThreadId { get; set; }
    public UserDetailsDto? SelectedThread { get; set; }

    /// <summary>
    /// This dictionary will store the ids of each thread the user is part of
    /// and the last 15 messages sent in each thread, this will be used when the app first loads to populate the contacts and t
    /// </summary>
    public Dictionary<Guid, ChatDetails> Threads { get; set; }
    public Dictionary<Guid, UserDetailsDto> Contacts { get; set; }
    public IEnumerable<ColorSchemeDto>? ColorSchemes { get; set; }

    public List<PendingRequestIncomingDto> IncomingContactRequests { get; set; }
    public List<PendingRequestSentDto> SentContactRequests { get; set; }

    public HubConnection ChatHubConnection { get; private set; } = default!;
    public HubConnection ContactsHubConnection { get; private set; } = default!;

    public AuthenticationState AuthState { get; set; } = default!;

    public string? CurrentUserId { get; set; }

    public string? UserJwt { get; set; }

    public ApplicationState(IJSRuntime js, AppSettings appSettings)
    {
        // * injected dependencies
        _appSettings = appSettings;
        _js = js;


        // * initializing properties
        IncomingContactRequests = new();
        SentContactRequests = new();
        Threads = new();
        Contacts = new();
        UserSettings = new();

        _chatHubUrl = $"{_appSettings.ServerBaseUrl}/chat-threads-hub";
        _contactsHubUrl = $"{_appSettings.ServerBaseUrl}/contacts-hub";
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
        .WithUrl(_chatHubUrl, o =>
        {
            o.AccessTokenProvider = () => Task.FromResult<string?>(UserJwt);
        })
        .Build();

        ChatHubConnection.On<MessageDto>("ReceiveMessageAsync", async message =>
        {
            Threads[message.ThreadId].Messages.Add(message);
            NotifyStateChanged();
            await _js.InvokeVoidAsync("playMessageSound", message.SenderId == CurrentUserId);
        });

        ChatHubConnection.On<DeleteMessagesRequest>("ReceiveDeletedMessagesIdsAsync", request =>
        {
            Threads[request.ThreadId].Messages.RemoveAll(m => request.MessagesIds!.Contains(m.Id));
            NotifyStateChanged();
        });

        await ChatHubConnection.StartAsync();
        await ChatHubConnection.InvokeAsync("JoinThreadsAsync");
    }

    private async Task InitContactsHubConnection()
    {
        ContactsHubConnection = new HubConnectionBuilder()
        .WithUrl(_contactsHubUrl, o =>
        {
            o.AccessTokenProvider = () => Task.FromResult<string?>(UserJwt);
        }).Build();

        ContactsHubConnection.On<PendingRequestIncomingDto>("ReceiveContactRequestAsync", async incomingRequest =>
        {
            IncomingContactRequests.Add(incomingRequest);
            NotifyStateChanged();
            await Task.Run(async () => await _js.InvokeVoidAsync("playRequestSound"));
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

        ContactsHubConnection.On<NewContactDto>("ReceiveNewContactAsync", async newContact =>
        {
            Contacts.Add(newContact.ThreadId, newContact.Contact);
            Threads?.Add(newContact.ThreadId, new ChatDetails
            {
                ChatThreadId = newContact.ThreadId,
                Messages = new(),
                Contact = newContact.Contact
            });

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
