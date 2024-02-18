namespace Flow.Server.Repositories;

public interface INotificationsRepository
{
    Task<Notification> AddNotificationAsync(Notification notification);
    Task<IEnumerable<Notification>> GetNotificationsAsync(CancellationToken cancellationToken, int loadNumber);
    Task<Notification> GetNotificationByIdAsync(Guid notificationId);
    Task DeleteNotificationAsync(Guid notificationId);
    Task MarkNewNotificationsAsSeenAsync();
}
