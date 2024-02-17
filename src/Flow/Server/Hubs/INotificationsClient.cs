namespace Flow.Server.Hubs;

public interface INotificationsClient
{
    Task ReceiveNotification(NotificationDto notification);
}
