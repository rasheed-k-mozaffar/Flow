using System.Runtime.CompilerServices;
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

    [Inject]
    public IThreadsService ThreadsService { get; set; } = default!;

    [Parameter] public EventCallback OnSearchButtonClicked { get; set; }
    [Parameter] public EventCallback OnContactsButtonClicked { get; set; }

    private List<PendingRequestIncomingDto> incomingContactRequests = new();
    private List<PendingRequestSentDto> sentContactRequests = new();

    private string errorMessage = string.Empty;
    private bool isDoneLoadingContactRequests = false;
    private bool isLoadingThreads = true;

    // * these variables will be used to determine which tab to display
    private bool displayChatsTab = true;
    private bool displayReqsTab = false;
    private bool displaySettingsTab = false;

    private Animate contactsTabAnimation = new();
    private Animate chatsTabAnimation = new();
    private Animate settingsTabAnimation = new();

    private void SearchButtonClicked() => OnSearchButtonClicked.InvokeAsync();

    protected override async Task OnInitializedAsync()
    {
        chatsTabAnimation.Run();

        var threadsTask = LoadThreadsAndMessagesAsync();
        var incomingReqsTask = LoadIncomingPendingContactRequestsAsync();
        var sentReqsTask = LoadSentPendingContactRequestsAsync();

        await Task.WhenAll(threadsTask, incomingReqsTask, sentReqsTask);

        isDoneLoadingContactRequests = true;
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

    private async Task LoadThreadsAndMessagesAsync()
    {
        var apiResponse = await ThreadsService.GetChatsAsync();

        if (apiResponse.IsSuccess)
        {
            AppState.Threads = apiResponse.Body!
            .OrderByDescending(kv => kv.Value.Messages.Any() ? kv.Value.Messages?.Max(m => m.SentOn) : DateTime.MinValue)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

            AppState.SelectedThreadId = AppState.Threads.First().Key;
            AppState.SelectedThread = AppState.Threads.First().Value;
            AppState.NotifyStateChanged();
        }

        isLoadingThreads = false;
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
        ResetSettingsState();
        displayChatsTab = false;
        displaySettingsTab = false;
        displayReqsTab = true;
        contactsTabAnimation.Run();
    }

    private void DisplayChatsTab()
    {
        ResetSettingsState();
        displayChatsTab = true;
        displaySettingsTab = false;
        displayReqsTab = false;
        chatsTabAnimation.Run();
    }

    private void DisplaySettingsTab()
    {
        ResetSettingsState();
        displayReqsTab = false;
        displayChatsTab = false;
        displaySettingsTab = true;
        settingsTabAnimation.Run();
    }
    #endregion

    private void ResetSettingsState()
    {
        AppState.SelectedSettings = null;
        AppState.NotifyStateChanged();
    }
}
