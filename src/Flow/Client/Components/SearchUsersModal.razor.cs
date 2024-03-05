using BlazorAnimate;
using Flow.Shared.ApiResponses;
using Microsoft.AspNetCore.Components;

namespace Flow.Client.Components;

public partial class SearchUsersModal : ComponentBase
{
    [Inject]
    public IUsersService UsersService { get; set; } = default!;

    [Inject]
    public IContactRequestsService ContactsService { get; set; } = default!;


    private CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(7.5));
    private ApiResponse<ICollection<UserSearchResultDto>> apiResponse = new();
    private IList<UserSearchResultDto> searchResults = new List<UserSearchResultDto>();

    private string searchTerm = string.Empty;
    private string errorMessage = string.Empty;
    private string successMessage = string.Empty;
    private bool isPerformingSearch = false;

    private async Task PerformSearchAsync()
    {
        try
        {
            apiResponse = await UsersService
                .SearchAsync(searchTerm, cancellationTokenSource.Token);

            if (apiResponse.IsSuccess)
            {
                searchResults = apiResponse.Body!.ToList();
            }
        }
        catch (ApiGetRequestFailedException ex)
        {
            errorMessage = ex.Message;
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            isPerformingSearch = false;
        }
    }

    private async Task HandlePerformingSearch(ChangeEventArgs e)
    {
        if (e.Value is not null)
        {
            string? newValue = e.Value.ToString();

            if (string.IsNullOrEmpty(newValue))
            {
                apiResponse = new();
                searchResults.Clear();
                cancellationTokenSource.Cancel();
                isPerformingSearch = false;
                searchTerm = string.Empty;
                return;
            }

            searchTerm = e.Value.ToString()!;
            searchResults.Clear();

            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            isPerformingSearch = true;
            await Task.Delay(500, cancellationTokenSource.Token);

            await PerformSearchAsync();
        }

        return;
    }

    private async Task SendRequestToUserAsync(string recipientId)
    {
        errorMessage = string.Empty;

        try
        {
            var requestResult = await ContactsService.SendContactRequestAsync(recipientId);

            if (requestResult.IsSuccess)
            {
                // * update the user's search result type
                var recipient = searchResults.First(r => r.UserId == recipientId);
                searchResults.Remove(recipient);
                recipient.UserType = SearchedUserType.PendingRequest;
                searchResults.Add(recipient);

                successMessage = $"Successfully sent contact request to {recipient.Name}";
                StateHasChanged();
            }
        }
        catch (OperationFailureException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private async Task CancelContactRequestAsync(Guid? requestId)
    {
        errorMessage = string.Empty;

        try
        {
            var cancellationResult = await ContactsService.CancelRequestAsync((Guid)requestId!);

            if (cancellationResult.IsSuccess)
            {
                // * change the state of the search result on the client
                var recipient = searchResults.First(r => r.ContactRequestId == requestId);
                searchResults.Remove(recipient);

                recipient.UserType = SearchedUserType.NonContact;
                searchResults.Add(recipient);
            }
        }
        catch (OperationFailureException ex)
        {
            errorMessage = ex.Message;
        }
    }

    private void CloseSuccessToast() => successMessage = string.Empty;
}
