using Flow.Shared.ApiResponses;
using Flow.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flow.Server.Controllers;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class ContactRequestsController : ControllerBase
{
    private readonly IContactRequestsRepository _contactRequestsRepository;
    private readonly ILogger<ContactRequestsController> _logger;
    private readonly UserInfo _userInfo;

    public ContactRequestsController
    (
        IContactRequestsRepository contactRequestsRepository,
        ILogger<ContactRequestsController> logger,
        UserInfo userInfo
    )
    {
        _contactRequestsRepository = contactRequestsRepository;
        _logger = logger;
        _userInfo = userInfo;
    }

    [HttpPost]
    [Route("send-request/{recipientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendContactRequest(string recipientId)
    {
        try
        {
            // ? Maybe a refactor? Maybe...
            var contactRequest = new ContactRequest()
            {
                Id = Guid.NewGuid(),
                SenderId = _userInfo.UserId!,
                RecipientId = recipientId,
            };

            await _contactRequestsRepository
                .SendConnectRequestAsync(recipientId, contactRequest);

            return Ok(new ApiResponse<Guid>
            {
                Message = "Contact request sent",
                Body = contactRequest.Id,
                IsSuccess = true
            });
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogError
            (
                "Contact request failed from {Name} due to recipient NOT being found",
                _userInfo.Name
            );

            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
        catch (ContactRequestOperationFailureException ex)
        {
            return BadRequest(new ApiErrorResponse
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

    [HttpGet]
    [Route("get-pending-requests")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPendingRequests([FromQuery] UserType userType)
    {
        try
        {
            var requests = await _contactRequestsRepository
                                .GetPendingContactRequestsAsync(userType);

            // * Map the response depending on the user's type
            if (userType is UserType.Sender)
            {
                var response = requests.Select(req => req.ToPendingContactRequestSentDto());

                return Ok(new ApiResponse<IEnumerable<PendingRequestSentDto>>
                {
                    Message = "Contact requests retrieved successfully",
                    Body = response,
                    IsSuccess = true
                });
            }
            else if (userType is UserType.Recipient)
            {
                var response = requests.Select(req => req.ToPendingContactRequestIncomingDto());

                return Ok(new ApiResponse<IEnumerable<PendingRequestIncomingDto>>
                {
                    Message = "Contact requests retrieved successfully",
                    Body = response,
                    IsSuccess = true
                });
            }
            else
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorMessage = "Incorrect request type"
                });
            }
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }

    [HttpPost]
    [Route("cancel-request/{requestId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelRequest(Guid requestId)
    {
        try
        {
            await _contactRequestsRepository
                .CancelConnectRequestAsync(requestId);

            return Ok(new ApiResponse
            {
                Message = "The request was cancelled",
                IsSuccess = true
            });
        }
        catch (ResourceNotFoundException ex)
        {
            // * request not found
            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
        catch (ContactRequestOperationFailureException ex)
        {
            // * database problem
            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }

    [HttpPost]
    [Route("resolve-request/{requestId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveRequest(Guid requestId, [FromQuery] RequestStatus newStatus)
    {
        try
        {
            await _contactRequestsRepository
                .ResolveConnectRequestAsync(requestId, newStatus);


            // create the response message based on the new status
            string responseMessage = newStatus switch
            {
                RequestStatus.Accepted => "A new contact was added to your contacts",
                RequestStatus.Declined => "The request was declined",
                _ => string.Empty
            };

            return Ok(new ApiResponse
            {
                Message = responseMessage,
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
        catch (InvalidContactOperationException ex)
        {
            // * this will happen in case the user tried replying to an already resolved request
            return BadRequest(new ApiErrorResponse
            {
                ErrorMessage = ex.Message
            });
        }
    }

    [HttpGet]
    [Route("get-contacts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetContacts()
    {
        Dictionary<Guid, AppUser> contacts = await _contactRequestsRepository.GetUserContactsAsync();
        Dictionary<Guid, UserDetailsDto> responseBody = new();

        foreach (var contact in contacts)
        {
            responseBody.Add(contact.Key, contact.Value.ToUserDetailsDto());
        }

        return Ok(new ApiResponse<Dictionary<Guid, UserDetailsDto>>
        {
            Message = "Contacts retrieved successfully",
            Body = responseBody,
            IsSuccess = true
        });
    }
}
