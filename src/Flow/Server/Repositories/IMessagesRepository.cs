namespace Flow.Server.Repositories;

public interface IMessagesRepository
{
    Task SaveMessageAsync(Message message);
    Task DeleteMessagesFromThreadAsync(Guid threadId, IEnumerable<Guid> idsOfMessagesToDelete);
}
