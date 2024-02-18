using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Flow.Server.Hubs;

[Authorize]
public class NotificationsHub : Hub<INotificationsClient>
{

}
