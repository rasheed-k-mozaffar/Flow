using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;

namespace Flow.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly ILogger<GroupsController> _logger;
    private readonly IGroupsRepository _groupsRepository;

    public GroupsController(ILogger<GroupsController> logger, IGroupsRepository groupsRepository)
    {
        _logger = logger;
        _groupsRepository = groupsRepository;
    }

    [HttpGet]
    [Route("get-group-details/{groupId}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse<GroupDetailsResponse>))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(UnauthorizedResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, type: typeof(ApiErrorResponse))]
    public async Task<IActionResult> GetGroupDetails(Guid groupId, CancellationToken cancellationToken)
    {
        try
        {
            var details = await _groupsRepository.GetGroupDetailsAsync(groupId, cancellationToken);

            return Ok(new ApiResponse<GroupDetailsResponse>
            {
                Message = "Group details retrieved successfully",
                Body = details,
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
    }

    [HttpPost]
    [Route("create-group")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(UnauthorizedResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status500InternalServerError, type: typeof(ApiErrorResponse))]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
    {
        var group = new ChatThread()
        {
            Name = request.GroupName,
            Description = request.Description,
        };

        try
        {
            var groupDetails = await _groupsRepository
                                                .CreateGroupAsync(group, request.Participants);

            return Ok(new ApiResponse<GroupDetailsResponse>
            {
                Message = "Group created successfully",
                Body = new GroupDetailsResponse
                {
                    GroupdThreadId = groupDetails.Id,
                    GroupName = groupDetails.Name ?? "Nameless Group",
                    Description = groupDetails.Description ?? "No description available",
                    CreatedAt = groupDetails.CreatedAt,
                    GroupParticipants = groupDetails
                                        .Participants
                                        .Select(p => p.ToUserDetailsDto())
                                        .ToList()
                },
                IsSuccess = true
            });
        }
        catch (DatabaseOperationFailedException ex)
        {
            return StatusCode(500, new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }

    [HttpPost]
    [Route("add-to-group")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(UnauthorizedResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, type: typeof(ApiErrorResponse))]
    public async Task<IActionResult> AddToGroup([FromBody] AddNewGroupParticipantsRequest request)
    {
        try
        {
            await _groupsRepository.AddNewParticipantsToGroupAsync(request.GroupId, request.NewParticipants);

            return Ok(new ApiResponse
            {
                Message = "New member(s) successfully added to the group",
                IsSuccess = true
            });
        }
        catch (ResourceNotFoundException ex)
        {
            _logger.LogError("Failed attempt to add new member to group. No group with ID: {groupId} was found.", request.GroupId);
            return NotFound(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }

    [HttpPost]
    [Route("leave-group/{groupId}/{participantId}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(UnauthorizedResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, type: typeof(ApiErrorResponse))]
    public async Task<IActionResult> RemoveFromGroup(Guid groupId, string participantId)
    {
        try
        {
            await _groupsRepository.LeaveGroupAsync(groupId, participantId);

            return Ok(new ApiResponse
            {
                Message = "Member removed successfully",
                IsSuccess = true
            });
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogError("Failed attempt to remove group member. No member with ID: {memberId} was found", participantId);
            return NotFound(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
        catch (ResourceNotFoundException ex)
        {
            _logger.LogError("Failed attempt to remove group member. No group with ID: {groupId} was found", groupId);
            return NotFound(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }

    [HttpDelete]
    [Route("delete-group/{groupId}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(UnauthorizedResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, type: typeof(ApiErrorResponse))]
    public async Task<IActionResult> DeleteGroup(Guid groupId, CancellationToken cancellationToken)
    {
        try
        {
            await _groupsRepository.DeleteGroupAsync(groupId, cancellationToken);

            return Ok(new ApiResponse
            {
                Message = "Group chat deleted successfully",
                IsSuccess = true
            });
        }
        catch (ResourceNotFoundException ex)
        {
            _logger.LogError("Failed attempt to delete group. No group with ID: {groupId} was found", groupId);
            return NotFound(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }

    [HttpPut]
    [Route("update-group/{groupdId}")]
    [ProducesResponseType(statusCode: StatusCodes.Status200OK, type: typeof(ApiResponse))]
    [ProducesResponseType(statusCode: StatusCodes.Status401Unauthorized, type: typeof(UnauthorizedResult))]
    [ProducesResponseType(statusCode: StatusCodes.Status404NotFound, type: typeof(ApiErrorResponse))]
    public async Task<IActionResult> UpdateGroup(Guid groupId, [FromBody] UpdateGroupDetailsRequest request)
    {
        try
        {
            await _groupsRepository.UpdateGroupDetailsAsync(groupId, request);

            return Ok(new ApiResponse
            {
                Message = "Group updated successfully",
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
    }
}