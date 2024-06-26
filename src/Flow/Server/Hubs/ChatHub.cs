﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Flow.Server.Hubs;

[Authorize]
public class ChatHub : Hub<IChatThreadsClient>
{
    private readonly UserInfo _userInfo;
    private readonly ILogger<ChatHub> _logger;
    private readonly IContactRequestsRepository _contactsRepository;
    private readonly IThreadRepository _threadsRepository;
    private readonly IMessagesRepository _messagesRepository;

    public ChatHub(UserInfo userInfo, ILogger<ChatHub> logger, IContactRequestsRepository contactsRepository, IMessagesRepository messagesRepository, IThreadRepository threadsRepository)
    {
        _userInfo = userInfo;
        _logger = logger;
        _contactsRepository = contactsRepository;
        _messagesRepository = messagesRepository;
        _threadsRepository = threadsRepository;
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        _logger.LogInformation
        (
            "User {username} joined the Chat Hub",
            _userInfo.Name
        );
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        _logger.LogInformation
        (
            "User {username} left the Chat Hub",
            _userInfo.Name
        );
    }

    /// <summary>
    /// This method will be invoked whenever a user types and sends a message, the invocation of this method 
    /// causes the propagation of the sent message to be received on the other end of the chat thread
    /// </summary>
    /// <param name="messageDto"></param>
    /// <returns></returns>
    public async Task SendMessageAsync(MessageDto messageDto)
    {
        await _messagesRepository.SaveMessageAsync(messageDto.ToMessage());
        await Clients.Group(messageDto.ThreadId.ToString()).ReceiveMessageAsync(messageDto.ToMessageReceivedDto());
    }

    public async Task JoinThreadAsync(Guid threadId)
    {
        _logger.LogWarning("User {username} joined Chat Thread {threadId}", _userInfo.Name, threadId);
        await Groups.AddToGroupAsync(Context.ConnectionId, threadId.ToString()!);
    }

    public async Task LeaveThreadAsync(Guid threadId)
    {
        _logger.LogWarning("User {username} left Chat Thread {threadId}", _userInfo.Name, threadId);
        await Groups.RemoveFromGroupAsync(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!, threadId.ToString()!);
    }


    #region On Connect Methods
    // * These two methods are going to only be used upon connecting to the hub

    public async Task JoinThreadsAsync()
    {
        var userChats = await _threadsRepository.GetChatThreadsAsync();

        // Join the user to their chat threads 
        foreach (var threadId in userChats.Keys)
        {
            _logger.LogWarning("User {username} joined Chat Thread {threadId}", _userInfo.Name, threadId);
            await Groups.AddToGroupAsync(Context.ConnectionId, threadId.ToString()!);
        }
    }

    public async Task LeaveThreadsAsync()
    {
        var userChats = await _threadsRepository.GetChatThreadsAsync();

        // Remove the user from their chat threads 
        foreach (var threadId in userChats.Keys)
        {
            _logger.LogWarning("User {username} left Chat Thread {threadId}", _userInfo.Name, threadId);
            await Groups.RemoveFromGroupAsync(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!, threadId.ToString()!);
        }
    }

    #endregion
}

public interface IChatThreadsClient
{
    Task SendMessageAsync(SendMessageDto message);
    Task ReceiveMessageAsync(MessageDto message);
    Task ReceiveDeletedMessagesIdsAsync(DeleteMessagesRequest deletedMessagesIds);
    Task ReceiveNewChatAsync(ChatDetails newChat);
    Task JoinThreadAsync(Guid threadId);
    Task LeaveThreasAsync(Guid threadId);
    Task JoinThreadsAsync();
    Task LeaveThreadsAsync();
}
