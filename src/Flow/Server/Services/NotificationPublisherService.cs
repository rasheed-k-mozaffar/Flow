
using Flow.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Flow.Server.Services;

public class NotificationPublisherService : INotificationPublisherService
{
    private readonly IHubContext<NotificationsHub, INotificationsClient> _hubContext;

    public NotificationPublisherService(IHubContext<NotificationsHub, INotificationsClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishNotificationToRecipientAsync(Notification notification)
    {
        await _hubContext
                .Clients
                .User(notification.RecipientId)
                .ReceiveNotification(notification.ToNotificationDto());
    }
}
