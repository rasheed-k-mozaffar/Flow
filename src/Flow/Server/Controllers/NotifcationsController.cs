using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationsRepository _notificationsRepository;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController
    (
        INotificationsRepository notificationsRepository,
        ILogger<NotificationsController> logger
    )
    {
        _notificationsRepository = notificationsRepository;
        _logger = logger;
    }

    [HttpGet]
    [Route("get-notifications")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse<IEnumerable<NotificationDto>>))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(EmptyResult))]
    public async Task<IActionResult> GetNotifications([FromQuery] int loadNumber, CancellationToken cancellationToken)
    {
        var notifications = await _notificationsRepository
                                .GetNotificationsAsync(cancellationToken, loadNumber);

        var notificationDtos = notifications.Select(n => n.ToNotificationDto());

        return Ok(new ApiResponse<IEnumerable<NotificationDto>>
        {
            Message = "Notifications retrieved successfully",
            Body = notificationDtos,
            IsSuccess = true
        });
    }

    [HttpPost]
    [Route("mark-notifications-as-seen")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(EmptyResult))]
    public async Task<IActionResult> MarkNotificationsAsSeen()
    {
        await _notificationsRepository.MarkNewNotificationsAsSeenAsync();

        return Ok(new ApiResponse
        {
            Message = "Notifications updated successfully",
            IsSuccess = true
        });
    }

    [HttpDelete]
    [Route("delete-notification/{notificationId}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(ApiErrorResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(EmptyResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, type: typeof(ApiErrorResponse))]
    public async Task<IActionResult> DeleteNotification(Guid notificationId)
    {
        try
        {
            await _notificationsRepository.DeleteNotificationAsync(notificationId);

            _logger.LogInformation("Notification with ID {notificationId} was deleted", notificationId);

            return Ok(new ApiResponse
            {
                Message = "Notification removed successfully",
                IsSuccess = true
            });
        }
        catch (ResourceNotFoundException ex)
        {
            return NotFound(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
        catch (DatabaseOperationFailedException ex)
        {
            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }
}
