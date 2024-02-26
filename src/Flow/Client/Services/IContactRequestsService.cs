using Flow.Shared.ApiResponses;

namespace Flow.Client.Services;

public interface IContactRequestsService
{
    Task<ApiResponse<IEnumerable<PendingRequestSentDto>>> GetSentPendingContactRequests();
    Task<ApiResponse<IEnumerable<PendingRequestIncomingDto>>> GetIncomingPendingContactRequests();
    Task<ApiResponse<IEnumerable<ContactDto>>> GetContactsAsync();
    Task<ApiResponse> SendContactRequestAsync(string recipientId);
    Task<ApiResponse> CancelRequestAsync(Guid requestId);
    Task<ApiResponse> ResolveRequestAsync(Guid requestId, RequestStatus newStatus);
}
