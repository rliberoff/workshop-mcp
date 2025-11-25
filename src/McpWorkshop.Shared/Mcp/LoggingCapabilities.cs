namespace McpWorkshop.Shared.Mcp;

/// <summary>
/// Represents logging-related server capabilities.
/// </summary>
public class LoggingCapabilities
{
    /// <summary>
    /// Gets or sets the logging levels supported by the server.
    /// </summary>
    public string[] Levels { get; set; } = ["DEBUG", "INFO", "WARN", "ERROR"];
}
