namespace McpWorkshop.Shared.Mcp;

/// <summary>
/// Represents a JSON-RPC 2.0 error object.
/// </summary>
public class JsonRpcError
{
    /// <summary>
    /// Gets or sets the error code. A number that indicates the error type that occurred.
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// Gets or sets a short description of the error.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional information about the error. This member is optional.
    /// </summary>
    public object? Data { get; set; }
}
