using Flow.Client.State;
using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Tokens;

namespace Flow.Client.Components;

public partial class ContactRequestsListView : ComponentBase
{
    [Inject]
    public ApplicationState AppState { get; set; } = default!;

    [Inject]
    public IContactRequestsService ContactsService { get; set; } = default!;

    [Parameter] public string? ErrorMessage { get; set; }
    [Parameter] public bool DisplayRequests { get; set; }

    private bool isMakingApiCall = false;
    private string errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        AppState.OnChange += StateHasChanged;
    }

    private async Task HandleCancellingContactRequestAsync(Guid requestId)
    {
        isMakingApiCall = true;
        errorMessage = string.Empty;

        try
        {
            var cancellationResult = await ContactsService.CancelRequestAsync(requestId);

            if (cancellationResult.IsSuccess)
            {
                var cancelledRequest = AppState.SentContactRequests.First(r => r.RequestId == requestId);
                AppState.SentContactRequests.Remove(cancelledRequest);
                AppState.NotifyStateChanged();
            }
        }
        catch (OperationFailureException ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isMakingApiCall = false;
        }
    }

    private async Task AcceptContactRequestAsync(Guid requestId)
    {
        var acceptanceSucceeded = await HandleResolvingContactRequestAsync(requestId, RequestStatus.Accepted);

        if (acceptanceSucceeded)
        {
            var resolvedRequest = AppState.IncomingContactRequests.First(r => r.RequestId == requestId);
            AppState.IncomingContactRequests.Remove(resolvedRequest);
            AppState.NotifyStateChanged();
        }
    }

    private async Task DeclineContactRequestAsync(Guid requestId)
    {
        var declineSucceeded = await HandleResolvingContactRequestAsync(requestId, RequestStatus.Declined);

        if (declineSucceeded)
        {
            var resolvedRequest = AppState.IncomingContactRequests.First(r => r.RequestId == requestId);
            AppState.IncomingContactRequests.Remove(resolvedRequest);
            AppState.NotifyStateChanged();
        }
    }

    private async Task<bool> HandleResolvingContactRequestAsync(Guid requestId, RequestStatus newStatus)
    {
        isMakingApiCall = true;
        errorMessage = string.Empty;

        var apiResponse = new ApiResponse();

        try
        {
            apiResponse = await ContactsService.ResolveRequestAsync(requestId, newStatus);
        }
        catch (OperationFailureException ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isMakingApiCall = false;
        }

        return apiResponse.IsSuccess;
    }
}
