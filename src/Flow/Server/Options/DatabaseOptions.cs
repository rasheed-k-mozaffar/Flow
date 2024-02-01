namespace Flow.Server.Options;

public class DatabaseOptions
{
    public string? ConnectionString { get; set; }
    public int CommandTimeoutDuration { get; set; }
    public bool EnableDetailedErrors { get; set; }
    public bool EnableSensetiveLogs { get; set; }

}
