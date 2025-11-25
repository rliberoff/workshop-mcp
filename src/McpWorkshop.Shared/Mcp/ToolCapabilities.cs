namespace McpWorkshop.Shared.Mcp;

/// <summary>
/// Represents tool-related server capabilities.
/// </summary>
public class ToolCapabilities
{
    /// <summary>
    /// Gets or sets a value indicating whether the server can notify clients when the list of tools changes.
    /// </summary>
    public bool ListChanged { get; set; } = false;
}
