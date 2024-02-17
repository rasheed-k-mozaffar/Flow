using Flow.Server.EventArguments;

namespace Flow.Server.Repositories;

public interface INotificationsRepository
{
    delegate Task NotificationAddedDelegate(object sender, NotificationEventArgs eventArgs);
    event NotificationAddedDelegate? OnNotificationAdded;
    Task<Notification> AddNotificationAsync(Notification notification);
    Task<IEnumerable<Notification>> GetNotificationsAsync(CancellationToken cancellationToken, int loadNumber);
    Task<Notification> GetNotificationByIdAsync(Guid notificationId);
    Task DeleteNotificationAsync(Guid notificationId);
    Task MarkNewNotificationsAsSeenAsync();
}
