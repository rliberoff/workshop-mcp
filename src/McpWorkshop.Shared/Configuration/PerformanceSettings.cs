namespace McpWorkshop.Shared.Configuration;

/// <summary>
/// Performance-related configuration settings.
/// </summary>
public class PerformanceSettings
{
    /// <summary>
    /// Gets or sets the target response time for resource reads in milliseconds.
    /// </summary>
    public int ResourceResponseTimeMs { get; set; } = 500;

    /// <summary>
    /// Gets or sets the target response time for tool execution in milliseconds.
    /// </summary>
    public int ToolResponseTimeMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the maximum logging overhead in milliseconds.
    /// </summary>
    public int MaxLoggingOverheadMs { get; set; } = 50;

    /// <summary>
    /// Gets or sets a value indicating whether performance tracking is enabled.
    /// </summary>
    public bool EnablePerformanceTracking { get; set; } = true;
}
