namespace McpWorkshop.Shared.Mcp;

/// <summary>
/// Standard JSON-RPC 2.0 error codes.
/// </summary>
public static class JsonRpcErrorCodes
{
    /// <summary>
    /// Invalid JSON was received by the server. An error occurred on the server while parsing the JSON text.
    /// </summary>
    public const int ParseError = -32700;

    /// <summary>
    /// The JSON sent is not a valid Request object.
    /// </summary>
    public const int InvalidRequest = -32600;

    /// <summary>
    /// The method does not exist or is not available.
    /// </summary>
    public const int MethodNotFound = -32601;

    /// <summary>
    /// Invalid method parameters.
    /// </summary>
    public const int InvalidParams = -32602;

    /// <summary>
    /// Internal JSON-RPC error.
    /// </summary>
    public const int InternalError = -32603;

    /// <summary>
    /// Custom error code: The request requires authentication.
    /// </summary>
    public const int Unauthorized = -32001;

    /// <summary>
    /// Custom error code: The authenticated user does not have permission to perform this operation.
    /// </summary>
    public const int Forbidden = -32002;

    /// <summary>
    /// Custom error code: The rate limit for requests has been exceeded.
    /// </summary>
    public const int RateLimitExceeded = -32003;
}
