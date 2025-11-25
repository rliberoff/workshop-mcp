namespace McpWorkshop.Shared.Mcp;

/// <summary>
/// Represents a JSON-RPC 2.0 request message.
/// </summary>
public class JsonRpcRequest
{
    /// <summary>
    /// Gets or sets the JSON-RPC protocol version. Must be "2.0".
    /// </summary>
    public string JsonRpc { get; set; } = "2.0";

    /// <summary>
    /// Gets or sets the name of the method to be invoked.
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the parameter values to be used during the invocation of the method.
    /// </summary>
    public object? Params { get; set; }

    /// <summary>
    /// Gets or sets the request identifier. This member is optional and can be null.
    /// </summary>
    public object? Id { get; set; }
}
