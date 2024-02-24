using System.Data;

namespace Flow.Shared.ApiResponses;

public class ApiResponse
{
    public string Message { get; set; } = null!;
    public bool IsSuccess { get; set; }
}

public class ApiResponse<T> : ApiResponse
{
    public T? Body { get; set; }
}
