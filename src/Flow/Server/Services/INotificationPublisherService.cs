namespace Flow.Server.Services;

public interface INotificationPublisherService
{
    /// <summary>
    /// Sends over a notification to a specific client using Signal R
    /// </summary>
    /// <param name="notification">The notification to send</param>
    /// <returns></returns>
    Task PublishNotificationToRecipientAsync(Notification notification);
}
