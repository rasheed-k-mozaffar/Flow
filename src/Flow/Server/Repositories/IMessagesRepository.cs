namespace Flow.Server.Repositories;

public interface IMessagesRepository
{
    Task SaveMessageAsync(Message message);
}
