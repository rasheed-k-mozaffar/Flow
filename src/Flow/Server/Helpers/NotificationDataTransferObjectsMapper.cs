namespace Flow.Server.Helpers;

public static class NotificationDataTransferObjectsMapper
{
    public static NotificationDto ToNotificationDto(this Notification notification)
    {
        return new()
        {
            Id = notification.Id,
            Content = notification.Content,
            Type = notification.Type,
            IssuedOn = notification.IssuedOn,
            Seen = notification.Seen,
            RecipientId = notification.RecipientId
        };
    }
}
