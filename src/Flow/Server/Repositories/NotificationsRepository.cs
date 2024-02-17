using Flow.Server.EventArguments;
using static Flow.Server.Repositories.INotificationsRepository;

namespace Flow.Server.Repositories;

public class NotificationsRepository : INotificationsRepository
{
    public event NotificationAddedDelegate? OnNotificationAdded;

    private const int NOTIFICATIONS_CHUNK_SIZE = 15;
    private readonly AppDbContext _db;
    private readonly UserInfo _userInfo;
    private readonly ILogger<NotificationsRepository> _logger;

    public NotificationsRepository
    (
        AppDbContext db,
        UserInfo userInfo,
        ILogger<NotificationsRepository> logger
    )
    {
        _db = db;
        _userInfo = userInfo;
        _logger = logger;
    }

    public async Task<Notification> AddNotificationAsync(Notification notification)
    {
        var entityEntry = await _db
                                .Notifications
                                .AddAsync(notification);

        if (entityEntry.State == EntityState.Added)
        {
            await _db.SaveChangesAsync();

            OnNotificationAdded?.Invoke(this, new() { Notification = notification });

            return notification;
        }
        else
        {
            _logger.LogError("Failed attempt to persist a new notification");
            throw new DatabaseOperationFailedException(message: "Something went wrong saving the notification");
        }
    }

    public async Task DeleteNotificationAsync(Guid notificationId)
    {
        var notification = await _db
                                .Notifications
                                .AsTracking()
                                .FirstOrDefaultAsync(n => (n.Id == notificationId && n.RecipientId == _userInfo.UserId));

        if (notification is null)
            throw new ResourceNotFoundException(message: "The notification was not found");

        var deletionResult = _db.Notifications.Remove(notification);

        if (deletionResult.State == EntityState.Deleted)
        {
            await _db.SaveChangesAsync();
            return;
        }

        _logger.LogError("Failed to delete notification with the ID: {notificationId}", notificationId);

        throw new DatabaseOperationFailedException(message: "Something went wrong while deleting the notification");
    }

    public async Task<Notification> GetNotificationByIdAsync(Guid notificationId)
    {
        var notification = await _db
                                .Notifications
                                .FirstOrDefaultAsync(n => n.Id == notificationId);

        if (notification is null)
            throw new ResourceNotFoundException(message: "The notification was not found");

        return notification;
    }

    /// <summary>
    /// Gets the notifications for the current authenticated user
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="loadNumber"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Notification>> GetNotificationsAsync(CancellationToken cancellationToken, int loadNumber)
    {
        var notifications = await _db
                                .Notifications
                                .Where(n => n.RecipientId == _userInfo.UserId)
                                .OrderByDescending(p => p.IssuedOn)
                                .Skip(loadNumber * NOTIFICATIONS_CHUNK_SIZE)
                                .Take(NOTIFICATIONS_CHUNK_SIZE)
                                .ToListAsync(cancellationToken);

        return notifications;
    }

    /// <summary>
    /// Updates unseen notifications to the "Seen" state
    /// </summary>
    /// <returns></returns>
    public async Task MarkNewNotificationsAsSeenAsync()
    {
        await _db.Notifications
            .Where(n => (n.RecipientId == _userInfo.UserId && !n.Seen))
            .ExecuteUpdateAsync
            (
                p => p.SetProperty(s => s.Seen, true)
            );

        await _db.SaveChangesAsync();
    }
}
