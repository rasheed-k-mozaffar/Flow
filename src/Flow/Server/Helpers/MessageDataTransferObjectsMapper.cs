namespace Flow.Server.Helpers
{
    public static class MessageDataTransferObjectsMapper
    {
        public static Message ToMessage(this SendMessageDto message)
        {
            return new()
            {
                SenderId = message.SenderId,
                ThreadId = message.ThreadId,
                Content = message.Content,
                SentOn = DateTime.UtcNow,
                Status = Shared.Enums.MessageStatus.Delivered,
            };
        }

        public static MessageDto ToMessageDto(this Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                ThreadId = message.ThreadId,
                SenderId = message.SenderId,
                Content = message.Content,
                SentOn = message.SentOn,
                Status = message.Status,
            };
        }

        public static MessageDto ToMessageReceivedDto(this SendMessageDto sentMessage)
        {
            return new()
            {
                Id = sentMessage.MessageId,
                ThreadId = sentMessage.ThreadId,
                Content = sentMessage.Content,
                SenderId = sentMessage.SenderId,
                Status = Shared.Enums.MessageStatus.Delivered,
                SentOn = DateTime.UtcNow
            };
        }
    }
}
