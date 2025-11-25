namespace McpWorkshop.Shared.Mcp;

/// <summary>
/// Represents MCP server information and capabilities.
/// </summary>
public class ServerInfo
{
    /// <summary>
    /// Gets or sets the name of the MCP server.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the MCP server.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MCP protocol version supported by this server.
    /// </summary>
    public string ProtocolVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the server capabilities.
    /// </summary>
    public ServerCapabilities? Capabilities { get; set; }
}
