namespace McpWorkshop.Shared.Mcp;

/// <summary>
/// Represents a JSON-RPC 2.0 response message.
/// </summary>
public class JsonRpcResponse
{
    /// <summary>
    /// Gets or sets the JSON-RPC protocol version. Must be "2.0".
    /// </summary>
    public string JsonRpc { get; set; } = "2.0";

    /// <summary>
    /// Gets or sets the result of the method invocation. This member is required on success and must not exist if there was an error.
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Gets or sets the error object in case the method invocation failed. This member is required on error and must not exist if there was no error.
    /// </summary>
    public JsonRpcError? Error { get; set; }

    /// <summary>
    /// Gets or sets the request identifier. This must be the same as the value of the id member in the request.
    /// </summary>
    public object? Id { get; set; }
}
