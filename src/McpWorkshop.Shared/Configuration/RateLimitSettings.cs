namespace McpWorkshop.Shared.Configuration;

/// <summary>
/// Rate limiting configuration settings.
/// </summary>
public class RateLimitSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether rate limiting is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of resource requests allowed per minute.
    /// </summary>
    public int ResourcesPerMinute { get; set; } = 100;

    /// <summary>
    /// Gets or sets the maximum number of tool execution requests allowed per minute.
    /// </summary>
    public int ToolsPerMinute { get; set; } = 50;

    /// <summary>
    /// Gets or sets the maximum number of requests allowed per minute for unauthenticated users.
    /// </summary>
    public int UnauthenticatedPerMinute { get; set; } = 10;
}
