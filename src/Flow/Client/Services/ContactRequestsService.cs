using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Flow.Shared.ApiResponses;
using Microsoft.Extensions.Primitives;

namespace Flow.Client.Services;

public class ContactRequestsService : IContactRequestsService
{
    #region API URLs
    private const string BASE_URL = "/api/contactrequests";
    private const string SEND_REQUEST_URL = $"{BASE_URL}/send-request";
    private const string PENDING_REQUESTS_URL = $"{BASE_URL}/get-pending-requests";
    private const string CANCEL_REQUEST_URL = $"{BASE_URL}/cancel-request";
    private const string RESOLVE_REQUEST_URL = $"{BASE_URL}/resolve-request";
    private const string CONTACTS_URL = $"{BASE_URL}/get-contacts";
    #endregion

    private readonly HttpClient _httpClient;

    public ContactRequestsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse> CancelRequestAsync(Guid requestId)
    {
        string requestUrl = $"{CANCEL_REQUEST_URL}/{requestId}";
        HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, null);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new OperationFailureException(errorResponse!.ErrorMessage);
        }

        var successResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return successResponse!;
    }

    public async Task<ApiResponse<IEnumerable<ContactDto>>> GetContactsAsync()
    {
        HttpResponseMessage response = await _httpClient.GetAsync(CONTACTS_URL);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new ApiGetRequestFailedException(errorResponse!.ErrorMessage);
        }

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<ContactDto>>>();
        return data!;
    }

    public async Task<ApiResponse<IEnumerable<PendingRequestIncomingDto>>> GetIncomingPendingContactRequests()
    {
        string requestUrl = $"{PENDING_REQUESTS_URL}?userType={UserType.Recipient}";
        HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new ApiGetRequestFailedException(errorResponse!.ErrorMessage);
        }

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PendingRequestIncomingDto>>>();
        return data!;
    }

    public async Task<ApiResponse<IEnumerable<PendingRequestSentDto>>> GetSentPendingContactRequests()
    {
        string requestUrl = $"{PENDING_REQUESTS_URL}?userType={UserType.Sender}";
        HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new ApiGetRequestFailedException(errorResponse!.ErrorMessage);
        }

        var data = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<PendingRequestSentDto>>>();
        return data!;
    }

    public async Task<ApiResponse> ResolveRequestAsync(Guid requestId, RequestStatus newStatus)
    {
        string requestUrl = $"{RESOLVE_REQUEST_URL}/{requestId}?newStatus={newStatus}";
        HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, null);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new OperationFailureException(errorResponse!.ErrorMessage);
        }

        var successResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return successResponse!;
    }

    public async Task<ApiResponse> SendContactRequestAsync(string recipientId)
    {
        string requestUrl = $"{SEND_REQUEST_URL}/{recipientId}";
        HttpResponseMessage response = await _httpClient.PostAsync(requestUrl, null);

        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            throw new OperationFailureException(errorResponse!.ErrorMessage);
        }

        var successResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
        return successResponse!;
    }
}