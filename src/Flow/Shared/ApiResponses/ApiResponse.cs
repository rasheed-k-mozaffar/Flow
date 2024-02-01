using System.Data;

namespace Flow.Shared.ApiResponses;

public class ApiResponse
{
    public required string Message { get; set; }
    public bool IsSuccess { get; set; }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Body { get; set; }
}
