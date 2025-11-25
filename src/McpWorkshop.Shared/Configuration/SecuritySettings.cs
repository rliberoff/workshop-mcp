namespace McpWorkshop.Shared.Configuration;

/// <summary>
/// Security-related configuration settings for the MCP workshop servers.
/// </summary>
public class SecuritySettings
{
    /// <summary>
    /// Gets or sets a value indicating whether authentication is required.
    /// </summary>
    public bool RequireAuthentication { get; set; } = false;

    /// <summary>
    /// Gets or sets the secret key used for JWT token generation.
    /// </summary>
    public string JwtSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the JWT token issuer identifier.
    /// </summary>
    public string JwtIssuer { get; set; } = "mcp-workshop";

    /// <summary>
    /// Gets or sets the JWT token audience identifier.
    /// </summary>
    public string JwtAudience { get; set; } = "mcp-client";

    /// <summary>
    /// Gets or sets the JWT token expiration time in minutes.
    /// </summary>
    public int JwtExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Gets or sets the rate limiting configuration.
    /// </summary>
    public RateLimitSettings RateLimit { get; set; } = new();
}
