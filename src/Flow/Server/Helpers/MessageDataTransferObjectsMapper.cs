namespace Flow.Server.Helpers
{
    public static class MessageDataTransferObjectsMapper
    {
        public static MessageDto ToMessageDto(this Message message)
        {
            return new MessageDto
            {
                Id = message.Id,
                ThreadId=message.ThreadId,
                SenderId=message.SenderId,
                Content = message.Content,
                SentOn = message.SentOn,
                Status = message.Status,
            };
        }
    }
}
