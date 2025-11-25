namespace McpWorkshop.Shared.Configuration;

/// <summary>
/// Represents server information and metadata.
/// </summary>
public class ServerInfo
{
    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string Name { get; set; } = "mcp-workshop-server";

    /// <summary>
    /// Gets or sets the server version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the MCP protocol version supported by this server.
    /// </summary>
    public string ProtocolVersion { get; set; } = "2024-11-05";

    /// <summary>
    /// Gets or sets the port number on which the server listens.
    /// </summary>
    public int Port { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the host address of the server.
    /// </summary>
    public string Host { get; set; } = "localhost";
}
