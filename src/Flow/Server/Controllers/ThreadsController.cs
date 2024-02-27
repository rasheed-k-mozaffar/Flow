using Flow.Server.Models;
using Flow.Shared.ApiResponses;
using Flow.Shared.DataTransferObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ThreadsController : ControllerBase
    {
        private readonly ThreadRepository _threads;
        public ThreadsController(ThreadRepository ThreadRepository) {
            _threads = ThreadRepository; 
        }
        [HttpGet]
        [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse<Dictionary<string, List<MessageDto>>>))]
        public async Task<IActionResult> GetNewestMessages()
        {
            var userThreadsMessages = await _threads.GetPreliminaryMessagesDTO();
            return Ok(new ApiResponse<Dictionary<string, List<MessageDto>>>
            {
                Message = "Messages retrieved successfully",
                Body = userThreadsMessages,
                IsSuccess = true
            });
        }
    }
}
