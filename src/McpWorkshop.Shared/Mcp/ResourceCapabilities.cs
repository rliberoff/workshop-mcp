namespace McpWorkshop.Shared.Mcp;

/// <summary>
/// Represents resource-related server capabilities.
/// </summary>
public class ResourceCapabilities
{
    /// <summary>
    /// Gets or sets a value indicating whether resource subscriptions are supported.
    /// </summary>
    public bool Subscribe { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the server can notify clients when the list of resources changes.
    /// </summary>
    public bool ListChanged { get; set; } = false;
}
