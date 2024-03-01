using Flow.Server.Models;
using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Server.Controllers;

[Route("/api/[controller]")]
[ApiController]
[Authorize]
public class ThreadsController : ControllerBase
{
    private readonly IThreadRepository _threadsRepository;
    public ThreadsController(IThreadRepository threadRepository)
    {
        _threadsRepository = threadRepository;
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
    [HttpPost]
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
}

