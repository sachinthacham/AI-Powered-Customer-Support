namespace SupportIQ.Infrastructure.Identity;

/// <summary>Configuration for JWT issuance and validation. Bound from the "Jwt" section.</summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Symmetric signing key - must be supplied via environment variable/user-secrets, never committed.</summary>
    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = "SupportIQ";

    public string Audience { get; set; } = "SupportIQ.Client";

    public int ExpiryMinutes { get; set; } = 60;
}
