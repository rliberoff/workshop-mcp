namespace McpWorkshop.Shared.Configuration;

/// <summary>
/// Configuration settings for MCP workshop servers.
/// Provides centralized configuration management for all exercises.
/// </summary>
public class WorkshopSettings
{
    public const string SectionName = "Workshop";

    /// <summary>
    /// Server information
    /// </summary>
    public ServerInfo Server { get; set; } = new();

    /// <summary>
    /// Security configuration
    /// </summary>
    public SecuritySettings Security { get; set; } = new();

    /// <summary>
    /// Performance settings
    /// </summary>
    public PerformanceSettings Performance { get; set; } = new();

    /// <summary>
    /// Data source connections
    /// </summary>
    public DataSourceSettings DataSources { get; set; } = new();
}

public class ServerInfo
{
    public string Name { get; set; } = "mcp-workshop-server";
    public string Version { get; set; } = "1.0.0";
    public string ProtocolVersion { get; set; } = "2024-11-05";
    public int Port { get; set; } = 5000;
    public string Host { get; set; } = "localhost";
}

public class SecuritySettings
{
    public bool RequireAuthentication { get; set; } = false;
    public string JwtSecret { get; set; } = string.Empty;
    public string JwtIssuer { get; set; } = "mcp-workshop";
    public string JwtAudience { get; set; } = "mcp-client";
    public int JwtExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Rate limiting configuration
    /// </summary>
    public RateLimitSettings RateLimit { get; set; } = new();
}

public class RateLimitSettings
{
    public bool Enabled { get; set; } = false;
    public int ResourcesPerMinute { get; set; } = 100;
    public int ToolsPerMinute { get; set; } = 50;
    public int UnauthenticatedPerMinute { get; set; } = 10;
}

public class PerformanceSettings
{
    /// <summary>
    /// Target response time for resource reads (ms)
    /// </summary>
    public int ResourceResponseTimeMs { get; set; } = 500;

    /// <summary>
    /// Target response time for tool execution (ms)
    /// </summary>
    public int ToolResponseTimeMs { get; set; } = 1000;

    /// <summary>
    /// Maximum logging overhead (ms)
    /// </summary>
    public int MaxLoggingOverheadMs { get; set; } = 50;

    /// <summary>
    /// Enable performance tracking
    /// </summary>
    public bool EnablePerformanceTracking { get; set; } = true;
}

public class DataSourceSettings
{
    public string SqlConnectionString { get; set; } = string.Empty;
    public string CosmosConnectionString { get; set; } = string.Empty;
    public string CosmosDatabase { get; set; } = "workshop";
    public string BlobStorageConnectionString { get; set; } = string.Empty;
    public string LocalDataPath { get; set; } = "./Data";
}
