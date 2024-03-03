namespace Flow.Server.Repositories;

public class MessagesRepository : IMessagesRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<MessagesRepository> _logger;

    public MessagesRepository(AppDbContext db, ILogger<MessagesRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task DeleteMessagesFromThreadAsync(Guid threadId, IEnumerable<Guid> idsOfMessagesToDelete)
    {
        int deletedEntitesCount = await _db
                                        .Messages
                                        .Where(m => m.ThreadId == threadId && idsOfMessagesToDelete.Contains(m.Id))
                                        .ExecuteDeleteAsync();
        if (deletedEntitesCount <= 0)
            throw new DatabaseOperationFailedException("Something went wrong while deleting the messages");
    }

    public async Task SaveMessageAsync(Message message)
    {
        var entityEntry = await _db
                                    .Messages
                                    .AddAsync(message);

        if (entityEntry.State is EntityState.Added)
        {
            _logger.LogInformation("A new message was saved to the database");
            await _db.SaveChangesAsync();
        }
    }
}
