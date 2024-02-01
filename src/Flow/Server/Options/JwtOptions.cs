namespace Flow.Server.Options;

public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public bool ValidateLifetime { get; set; }
    public bool ValidateIssuer { get; set; }
    public bool ValidateAudience { get; set; }
    public bool RequiresExpirationTime { get; set; }
    public int ExpiresIn { get; set; }
}
