﻿using System.Runtime.CompilerServices;
using BlazorAnimate;
using Flow.Client.State;
using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Components;

namespace Flow.Client.Components;

public partial class SideBar : ComponentBase
{
    [Inject]
    public IContactRequestsService ContactsService { get; set; } = default!;

    [Inject]
    public ApplicationState AppState { get; set; } = default!;

    [Parameter] public EventCallback OnSearchButtonClicked { get; set; }
    [Parameter] public EventCallback OnContactsButtonClicked { get; set; }

    private List<PendingRequestIncomingDto> incomingContactRequests = new();
    private List<PendingRequestSentDto> sentContactRequests = new();

    private bool isMakingApiCall = false; // * this will be used to disable buttons
    private string errorMessage = string.Empty;
    private bool isLoadingContacts = true;
    private bool isDoneLoadingContactRequests = false;

    // * these variables will be used to determine which tab to display
    private bool displayReqsTab = false;

    private Animate contactsTabAnimation = new();
    private Animate chatsTabAnimation = new();

    private void SearchButtonClicked() => OnSearchButtonClicked.InvokeAsync();

    protected override async Task OnInitializedAsync()
    {
        chatsTabAnimation.Run();

        var contactsTask = LoadContactsAsync();
        var incomingReqsTask = LoadIncomingPendingContactRequestsAsync();
        var sentReqsTask = LoadSentPendingContactRequestsAsync();

        await Task.WhenAll(contactsTask, incomingReqsTask, sentReqsTask);

        isDoneLoadingContactRequests = true;
    }

    private async Task LoadContactsAsync()
    {
        var apiResponse = await ContactsService.GetContactsAsync();

        if (apiResponse.IsSuccess)
        {
            AppState.Contacts = apiResponse.Body!.ToList();
        }

        isLoadingContacts = false;
    }

    private async Task LoadSentPendingContactRequestsAsync()
    {
        try
        {
            var apiResponse = await ContactsService.GetSentPendingContactRequests();

            if (apiResponse.IsSuccess)
            {
                AppState.SentContactRequests = apiResponse.Body!.ToList();
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
                AppState.IncomingContactRequests = apiResponse.Body!.ToList();
            }
        }
        catch (ApiGetRequestFailedException ex)
        {
            errorMessage = ex.Message;
        }
    }

    #region Animation Methods
    private void DisplayContactsTab()
    {
        displayReqsTab = true;
        contactsTabAnimation.Run();
    }

    private void DisplayChatsTab()
    {
        displayReqsTab = false;
        chatsTabAnimation.Run();
    }
    #endregion
}
