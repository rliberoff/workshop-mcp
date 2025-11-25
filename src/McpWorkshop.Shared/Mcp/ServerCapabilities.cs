namespace McpWorkshop.Shared.Mcp;

/// <summary>
/// Represents the capabilities supported by an MCP server.
/// </summary>
public class ServerCapabilities
{
    /// <summary>
    /// Gets or sets the resource-related capabilities.
    /// </summary>
    public ResourceCapabilities? Resources { get; set; }

    /// <summary>
    /// Gets or sets the tool-related capabilities.
    /// </summary>
    public ToolCapabilities? Tools { get; set; }

    /// <summary>
    /// Gets or sets the logging-related capabilities.
    /// </summary>
    public LoggingCapabilities? Logging { get; set; }
}
