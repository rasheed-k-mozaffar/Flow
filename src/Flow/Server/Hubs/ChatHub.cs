using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Flow.Server.Hubs;

[Authorize]
public class ChatHub : Hub<IChatThreadsClient>
{
    private readonly UserInfo _userInfo;
    private readonly ILogger<ChatHub> _logger;
    private readonly IContactRequestsRepository _contactsRepository;
    private readonly IMessagesRepository _messagesRepository;

    public ChatHub(UserInfo userInfo, ILogger<ChatHub> logger, IContactRequestsRepository contactsRepository, IMessagesRepository messagesRepository)
    {
        _userInfo = userInfo;
        _logger = logger;
        _contactsRepository = contactsRepository;
        _messagesRepository = messagesRepository;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _logger.LogInformation
        (
            "User {username} joined the Chat Hub",
            _userInfo.Name
        );

        var contacts = await _contactsRepository.GetUserContactsAsync();

        var threadsIds = contacts.Select(p => p.ChatThreadId);

        await JoinThreadsAsync(threadsIds);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        _logger.LogInformation
        (
            "User {username} left the Chat Hub",
            _userInfo.Name
        );

        var contacts = await _contactsRepository.GetUserContactsAsync();

        var threadsIds = contacts.Select(p => p.ChatThreadId);

        await LeaveThreadsAsync(threadsIds);
    }

    /// <summary>
    /// This method will be invoked whenever a user types and sends a message, the invocation of this method causes the propagation of the
    /// sent message to be received on the other end of the chat thread
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task SendMessageAsync(SendMessageDto message)
    {
        await _messagesRepository.SaveMessageAsync(message.ToMessage());
        await Clients.Group(message.ThreadId.ToString()).ReceiveMessageAsync(message.ToMessageReceivedDto());
    }

    public async Task JoinThreadsAsync(IEnumerable<Guid?> threads)
    {
        // Join the user to their chat threads 
        foreach (var threadId in threads)
        {
            _logger.LogWarning("User {username} joined Chat Thread {threadId}", _userInfo.Name, threadId);
            await Groups.AddToGroupAsync(Context.ConnectionId, threadId.ToString()!);
        }
    }

    public async Task LeaveThreadsAsync(IEnumerable<Guid?> threads)
    {
        // Remove the user from their chat threads 
        foreach (var threadId in threads)
        {
            _logger.LogWarning("User {username} left Chat Thread {threadId}", _userInfo.Name, threadId);
            await Groups.RemoveFromGroupAsync(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!, threadId.ToString()!);
        }
    }
}

public interface IChatThreadsClient
{
    Task SendMessageAsync(SendMessageDto message);
    Task ReceiveMessageAsync(MessageDto message);
    Task ReceiveDeletedMessagesIdsAsync(DeleteMessagesRequest deletedMessagesIds);
    Task JoinThreadsAsync(Guid? threadId);
    Task LeaveThreadsAsync(Guid? threadId);
}
