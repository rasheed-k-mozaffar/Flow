﻿
using Flow.Server.Extensions;
using Flow.Server.Hubs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace Flow.Server.Repositories;

public class GroupsRepository : IGroupsRepository
{
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly IHubContext<ContactRequestsHub, IContactsClient> _contactsHubContext;

    public GroupsRepository(AppDbContext db, UserManager<AppUser> userManager, IHubContext<ContactRequestsHub, IContactsClient> contactsHubContext)
    {
        _db = db;
        _userManager = userManager;
        _contactsHubContext = contactsHubContext;
    }

    public async Task<ChatThread> CreateGroupAsync(ChatThread groupDetails, ICollection<string> participants, string? groupPictureUrl = null)
    {
        var groupParticipants = await _userManager
                                        .Users
                                        .Where(u => participants.Contains(u.Id))
                                        .ToListAsync();

        var group = new ChatThread
        {
            Id = groupDetails.Id,
            Name = groupDetails.Name,
            Description = groupDetails.Description,
            Type = Shared.Enums.ThreadType.Group,
            CreatedAt = DateTime.UtcNow,
            Participants = groupParticipants,
        };

        var creationResult = _db
                                .Threads
                                .Add(group);

        if (creationResult.State == EntityState.Added)
        {
            await _db.SaveChangesAsync();

            await _contactsHubContext
                    .Clients
                    .Users(participants)
                    .ReceiveNewChatAsync(new ChatDetails
                    {
                        ChatThreadId = group.Id,
                        GroupName = group.Name,
                        GroupDescription = group.Description,
                        Participants = groupParticipants
                                            .Select(user => user.ToUserDetailsDto())
                                            .ToList(),
                        Type = group.Type,
                        Messages = new List<MessageDto>(),
                        GroupImageUrl = groupPictureUrl
                    });

            return group;
        }

        throw new DatabaseOperationFailedException("Failed to create a new group chat");
    }

    public async Task DeleteGroupAsync(Guid groupThreadId, CancellationToken cancellationToken)
    {
        var group = await _db.FindAsyncAndThrowIfNull<ChatThread, ResourceNotFoundException>
                (
                    groupThreadId,
                    e => new ResourceNotFoundException("Group was not found"),
                    cancellationToken
                );

        // * Delete the messages in the group chat
        var messages = await _db
                                .Messages
                                .Where(m => m.ThreadId == groupThreadId)
                                .ExecuteDeleteAsync(cancellationToken);

        var entityEntry = _db.Threads.Remove(group);

        if (entityEntry.State is EntityState.Deleted)
        {
            await _db.SaveChangesAsync();

            // TODO: Notify group members about the deletion
        }

        throw new DatabaseOperationFailedException("Failed to delete group. Worry not! it's on us!");
    }

    public async Task LeaveGroupAsync(Guid groupThreadId, string participantId)
    {
        var participant = await _userManager
                                .FindByIdAsync(participantId);

        if (participant is null)
        {
            throw new UserNotFoundException("Group member was not found!");
        }

        try
        {
            var group = await _db.FindAsyncAndThrowIfNull<ChatThread, ResourceNotFoundException>
                    (
                        groupThreadId,
                        e => new ResourceNotFoundException("Group was not found")
                    );

            // remove the member
            group.Participants.Remove(participant);

            // TODO: Notify group partcipant

            await _db.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<GroupDetailsResponse> GetGroupDetailsAsync(Guid groupThreadId, CancellationToken cancellationToken)
    {

        var group = await _db
                            .Threads
                            .Include(p => p.Participants)
                            .FirstOrDefaultAsync(g => g.Id == groupThreadId, cancellationToken);

        if (group is null)
        {
            throw new ResourceNotFoundException("Group was not found!");
        }

        return new GroupDetailsResponse
        {
            GroupThreadId = group.Id,
            GroupName = group.Name ?? "Nameless Group",
            Description = group.Description ?? "No description available",
            GroupParticipants = group.Participants.Select(u => u.ToUserDetailsDto()).ToList(),
            CreatedAt = group.CreatedAt
        };

    }

    public async Task AddNewParticipantsToGroupAsync(Guid groupThreadId, ICollection<string> newParticipants)
    {
        var group = await _db.FindAsyncAndThrowIfNull<ChatThread, ResourceNotFoundException>(
                    groupThreadId,
                    e => new ResourceNotFoundException("Group was not found")
                );

        var participants = await _userManager
                                    .Users
                                    .Where(u => newParticipants.Contains(u.Id))
                                    .ToListAsync();

        var existingParticipants = group
                                    .Participants
                                    .Select(p => p.Id);

        foreach (var participant in participants)
        {
            if (existingParticipants.Contains(participant.Id))
            {
                continue; // Skip already existing participants
            }
            else
            {
                group.Participants.Add(participant);
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task UpdateGroupDetailsAsync(Guid groupThreadId, UpdateGroupDetailsRequest request)
    {
        var group = await _db.FindAsyncAndThrowIfNull<ChatThread, ResourceNotFoundException>
                (
                    groupThreadId,
                    e => new ResourceNotFoundException("Group was not found")
                );


        group.Name = request.NewGroupName;
        group.Description = request.NewGroupDescription;

        await _db.SaveChangesAsync();
    }
}