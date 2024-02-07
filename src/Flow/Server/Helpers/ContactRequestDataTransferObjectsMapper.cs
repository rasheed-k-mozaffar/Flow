namespace Flow.Server.Helpers;

public static class ContactRequestDataTransferObjectsMapper
{
    /// <summary>
    /// Maps a contact request made by the sender to a PendingRequestDto
    /// This is meant to be used when the user wants to see the other users to whom the user's sent contact requests
    /// </summary>
    /// <param name="request">The pending contact request sent by the user</param>
    /// <returns></returns>
    public static PendingRequestSentDto ToPendingContactRequestSentDto(this ContactRequest request)
    {
        return new PendingRequestSentDto
        {
            RequestId = request.Id,
            Recipient = request.Recipient.ToUserDetailsDto()
        };
    }

    /// <summary>
    /// Maps a contact request made by a different user to the user trying to view who's sent them a contact request
    /// </summary>
    /// <param name="request">The pending contact request incoming from a different user</param>
    /// <returns></returns>
    public static PendingRequestIncomingDto ToPendingContactRequestIncomingDto(this ContactRequest request)
    {
        return new PendingRequestIncomingDto
        {
            RequestId = request.Id,
            Sender = request.Sender.ToUserDetailsDto()
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public static ContactDto ToContactDto(this ContactRequest request, string userId)
    {
        if (request.SenderId == userId)
        {
            // the contact to be mapped, is the recipient
            return new ContactDto
            {
                RequestId = request.Id,
                ThreadId = request.ChatThreadId,
                Contact = request.Recipient.ToUserDetailsDto()
            };
        }
        else
        {
            // the contact to be mapped, is the sender
            return new ContactDto
            {
                RequestId = request.Id,
                ThreadId = request.ChatThreadId,
                Contact = request.Sender.ToUserDetailsDto()
            };
        }
    }
}
