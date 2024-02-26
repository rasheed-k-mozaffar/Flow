﻿using System.Runtime.CompilerServices;
using BlazorAnimate;
using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Components;

namespace Flow.Client.Components;

public partial class SideBar : ComponentBase
{
    [Inject]
    public IContactRequestsService ContactsService { get; set; } = default!;

    [Parameter] public EventCallback OnSearchButtonClicked { get; set; }
    [Parameter] public EventCallback OnContactsButtonClicked { get; set; }
    [Parameter] public EventCallback OnNotificationsButtonClicked { get; set; }

    private IEnumerable<ContactDto> contacts = new List<ContactDto>();
    private List<PendingRequestIncomingDto> incomingContactRequests = new();
    private List<PendingRequestSentDto> sentContactRequests = new();
    private TimeOnly lastContactRequestsRefreshTime;

    private bool isMakingApiCall = false; // * this will be used to disable buttons
    private string errorMessage = string.Empty;
    private bool isLoadingContacts = true;
    private bool isLoadingContactRequests = true;

    // * these variables will be used to determine which tab to display
    private bool displayNotificationsTab = false;
    private bool displayContactsTab = false;

    private Animate contactsTabAnimation = new();
    private Animate notificationsTabAnimation = new();
    private Animate chatsTabAnimation = new();

    private void SearchButtonClicked() => OnSearchButtonClicked.InvokeAsync();

    protected override async Task OnInitializedAsync()
    {
        chatsTabAnimation.Run();
        await LoadContactsAsync();

        await LoadIncomingPendingContactRequestsAsync();
        await LoadSentPendingContactRequestsAsync();
        lastContactRequestsRefreshTime = TimeOnly.FromDateTime(DateTime.Now.ToLocalTime());

        isLoadingContactRequests = false;
    }

    // TODO: Load the notifications after the side bar renders
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadContactsAsync()
    {
        var apiResponse = await ContactsService.GetContactsAsync();

        if (apiResponse.IsSuccess)
        {
            contacts = apiResponse.Body!.ToList();
        }

        await Task.Delay(500);

        isLoadingContacts = false;
    }

    private async Task RefreshContactRequestsAsync()
    {
        await LoadSentPendingContactRequestsAsync();
        await LoadIncomingPendingContactRequestsAsync();
        lastContactRequestsRefreshTime = TimeOnly.FromDateTime(DateTime.Now.ToLocalTime());
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadSentPendingContactRequestsAsync()
    {
        try
        {
            var apiResponse = await ContactsService.GetSentPendingContactRequests();

            if (apiResponse.IsSuccess)
            {
                sentContactRequests = apiResponse.Body!.ToList();
            }
        }
        catch (ApiGetRequestFailedException ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task LoadIncomingPendingContactRequestsAsync()
    {
        try
        {
            var apiResponse = await ContactsService.GetIncomingPendingContactRequests();

            if (apiResponse.IsSuccess)
            {
                incomingContactRequests = apiResponse.Body!.ToList();
            }
        }
        catch (ApiGetRequestFailedException ex)
        {
            errorMessage = ex.Message;
        }
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
                var cancelledRequest = sentContactRequests.First(r => r.RequestId == requestId);
                sentContactRequests.Remove(cancelledRequest);
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
            var resolvedRequest = incomingContactRequests.First(r => r.RequestId == requestId);
            incomingContactRequests.Remove(resolvedRequest);
        }
    }

    private async Task DeclineContactRequestAsync(Guid requestId)
    {
        var declineSucceeded = await HandleResolvingContactRequestAsync(requestId, RequestStatus.Declined);

        if (declineSucceeded)
        {
            var resolvedRequest = incomingContactRequests.First(r => r.RequestId == requestId);
            incomingContactRequests.Remove(resolvedRequest);
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

    #region Animation Methods
    private void DisplayContactsTab()
    {
        displayNotificationsTab = false;
        displayContactsTab = true;
        contactsTabAnimation.Run();
    }
    private void DisplayNotificationsTab()
    {
        displayContactsTab = false;
        displayNotificationsTab = true;
        notificationsTabAnimation.Run();
    }

    private void DisplayChatsTab()
    {
        displayNotificationsTab = false;
        displayContactsTab = false;
        chatsTabAnimation.Run();
    }
    #endregion
}
