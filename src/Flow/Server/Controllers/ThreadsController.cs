using Flow.Server.Hubs;
using Flow.Server.Models;
using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Flow.Server.Controllers;

[Route("/api/[controller]")]
[ApiController]
[Authorize]
public class ThreadsController : ControllerBase
{
    private readonly IThreadRepository _threadsRepository;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IHubContext<ChatHub, IChatThreadsClient> _chatHubContext;
    private readonly ILogger<ThreadsController> _logger;
    public ThreadsController
    (
        IThreadRepository threadRepository,
        IHubContext<ChatHub, IChatThreadsClient> chatHubContext,
        ILogger<ThreadsController> logger,
        IMessagesRepository messagesRepository
    )
    {
        _threadsRepository = threadRepository;
        _chatHubContext = chatHubContext;
        _logger = logger;
        _messagesRepository = messagesRepository;
    }

    [HttpGet]
    [Route("get-latest-messages")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse<Dictionary<string, List<MessageDto>>>))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(UnauthorizedResult))]
    public async Task<IActionResult> GetLatestMessages()
    {
        var userThreadsMessages = await _threadsRepository.GetPreliminaryMessagesForUserChatThreads();

        return Ok(new ApiResponse<Dictionary<string, List<MessageDto>>>
        {
            Message = "Messages retrieved successfully",
            Body = userThreadsMessages,
            IsSuccess = true
        });
    }

    [HttpGet]
    [Route("get-messages-by-date")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse<List<MessageDto>>))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(UnauthorizedResult))]
    public async Task<IActionResult> GetChatMessages([FromBody] MessagesRequestDto request)
    {
        var LoadedMessages = await _threadsRepository.GetMessagesByDate(request);

        return Ok(new ApiResponse<List<MessageDto>>
        {
            Message = "Messages retrieved",
            IsSuccess = true,
            Body = LoadedMessages
        });
    }

    [HttpPost]
    [Route("delete-messages")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status400BadRequest, type: typeof(ApiErrorResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError, type: typeof(ApiErrorResponse))]
    public async Task<IActionResult> DeleteMessages([FromBody] DeleteMessagesRequest request)
    {
        try
        {
            if (request.MessagesIds is null || !request.MessagesIds.Any())
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorMessage = "You haven't selected any messages to delete"
                });
            }

            await _messagesRepository.DeleteMessagesFromThreadAsync(request.ThreadId, request.MessagesIds);

            // propagate the ids to the client
            await _chatHubContext
                    .Clients
                    .Group(request.ThreadId.ToString())
                    .ReceiveDeletedMessagesIdsAsync(request);

            return Ok(new ApiResponse
            {
                Message = "Messages deleted successfully",
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
        catch (IOException ex)
        {
            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }
}

