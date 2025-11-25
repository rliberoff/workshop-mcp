namespace McpWorkshop.Shared.Configuration;

/// <summary>
/// Configuration settings for MCP workshop servers.
/// Provides centralized configuration management for all exercises.
/// </summary>
public class WorkshopSettings
{
    /// <summary>
    /// The configuration section name for workshop settings.
    /// </summary>
    public const string SectionName = "Workshop";

    /// <summary>
    /// Gets or sets the server information.
    /// </summary>
    public ServerInfo Server { get; set; } = new();

    /// <summary>
    /// Gets or sets the security configuration.
    /// </summary>
    public SecuritySettings Security { get; set; } = new();

    /// <summary>
    /// Gets or sets the performance settings.
    /// </summary>
    public PerformanceSettings Performance { get; set; } = new();

    /// <summary>
    /// Gets or sets the data source connections.
    /// </summary>
    public DataSourceSettings DataSources { get; set; } = new();
}
