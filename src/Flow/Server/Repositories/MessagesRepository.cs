namespace Flow.Server.Repositories;

public class MessagesRepository : IMessagesRepository
{
    private readonly AppDbContext _db;
    private readonly ILogger<MessagesRepository> _logger;
    private readonly IFilesRepository _filesRepository;

    public MessagesRepository(AppDbContext db, ILogger<MessagesRepository> logger, IFilesRepository filesRepository)
    {
        _db = db;
        _logger = logger;
        _filesRepository = filesRepository;
    }

    public async Task DeleteMessagesFromThreadAsync(Guid threadId, IEnumerable<Guid> idsOfMessagesToDelete)
    {
        var messagesToDelete = await _db
                                        .Messages
                                        .Where(m => m.ThreadId == threadId && idsOfMessagesToDelete.Contains(m.Id))
                                        .ToListAsync();

        // * For image messages, delete the images from the server as well  
        foreach (var message in messagesToDelete)
        {
            if (message.Type is not Shared.Enums.MessageType.Image)
                continue;

            try
            {
                var removalSucceeded = await _filesRepository.RemoveImageByRelativeUrlAsync(message.Content);
            }
            catch (ResourceNotFoundException)
            {
                throw;
            }
            catch (IOException)
            {
                throw;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
        }

        _db.Messages.RemoveRange(messagesToDelete);
        await _db.SaveChangesAsync();
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
