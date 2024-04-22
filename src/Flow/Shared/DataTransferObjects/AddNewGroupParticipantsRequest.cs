namespace Flow.Shared.DataTransferObjects;

public record AddNewGroupParticipantsRequest(Guid GroupId, ICollection<string> NewParticipants);
