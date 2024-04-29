using Flow.Client.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Internal;

namespace Flow.Client.Components;

public partial class CreateGroupChatModal : ComponentBase
{
    [Inject]
    public IGroupsService GroupsService { get; set; } = default!;

    [Parameter] public bool Show { get; set; }
    [Parameter] public EventCallback OnCloseClicked { get; set; }

    private CreateGroupRequest request = new();
    private List<UserDetailsDto> userContacts = new();
    private List<UserDetailsDto> fliteredContacts = new();
    private HashSet<string> selectedContacts = new();

    private string? searchTerm = null;
    private string _errorMessage = string.Empty;
    private bool _isMakingApiCall = false;

    protected override async Task OnParametersSetAsync()
    {
        userContacts = AppState.Threads.Values
                            .Where(thread => thread.Type == ThreadType.Normal)
                            .SelectMany(thread => thread.Participants)
                            .Distinct()
                            .Where(user => user.UserId != AppState.CurrentUserId)
                            .OrderBy(user => user.Name)
                            .ToList();

        fliteredContacts = userContacts;

        await base.OnParametersSetAsync();
    }

    private async Task HandleCreatingGroupAsync()
    {
        try
        {
            _isMakingApiCall = true;
            _errorMessage = string.Empty;

            if (selectedContacts.Count < 1)
            {
                _errorMessage = "You must select at least one contact";
                return;
            }

            request.Participants = selectedContacts.ToList();
            request.Participants.Add(AppState.CurrentUserId!);
            var apiResponse = await GroupsService.CreateGroupAsync(request);

            if (apiResponse.IsSuccess)
            {
                Show = false;
                return;
            }
        }
        catch (OperationFailureException ex)
        {
            _errorMessage = ex.Message;
        }
        finally
        {
            _isMakingApiCall = false;
        }
    }

    private void OnSearchValueChanged(ChangeEventArgs args)
    {
        searchTerm = args.Value?.ToString();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            fliteredContacts = userContacts
                                .Where(user => user.Name!.ToLower().Contains(searchTerm.ToLower()))
                                .ToList();
        }
        else
        {
            fliteredContacts = userContacts;
        }
    }

    private void CloseButtonClicked()
    {
        OnCloseClicked.InvokeAsync();
    }

    private void CloseErrorAlert() => _errorMessage = string.Empty;

    private void OnContactClicked(string selectedUser)
    {
        if (selectedContacts.Contains(selectedUser))
        {
            selectedContacts.Remove(selectedUser);
        }
        else
        {
            selectedContacts.Add(selectedUser);
        }
        StateHasChanged();
    }
}
