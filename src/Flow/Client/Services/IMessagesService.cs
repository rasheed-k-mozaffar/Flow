namespace Flow.Client.Services;

public interface IMessagesService
{
    Task SendDeleteMessagesRequestAsync(DeleteMessagesRequest request);
}
