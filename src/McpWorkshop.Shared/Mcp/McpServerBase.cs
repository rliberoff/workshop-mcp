using McpWorkshop.Shared.Logging;

using Microsoft.Extensions.Options;

namespace McpWorkshop.Shared.Mcp;

/// <summary>
/// Base class for MCP servers providing common functionality.
/// Implements MCP-First design principle (Constitution I).
/// </summary>
public abstract class McpServerBase
{
    private readonly IStructuredLogger logger;

    private readonly Configuration.WorkshopSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="McpServerBase"/> class.
    /// </summary>
    /// <param name="logger">The structured logger instance.</param>
    /// <param name="settings">The workshop settings.</param>
    protected McpServerBase(
        IStructuredLogger logger,
        IOptions<Configuration.WorkshopSettings> settings)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Gets the structured logger instance.
    /// </summary>
    protected IStructuredLogger Logger => logger;

    /// <summary>
    /// Gets the workshop settings.
    /// </summary>
    protected Configuration.WorkshopSettings Settings => settings;

    /// <summary>
    /// Generates a unique request ID for tracking.
    /// </summary>
    /// <returns>A unique request identifier.</returns>
    public static string GenerateRequestId()
    {
        return Guid.NewGuid().ToString("N")[..12];
    }

    /// <summary>
    /// Validates JSON-RPC 2.0 request format.
    /// </summary>
    /// <param name="request">The JSON-RPC request to validate.</param>
    /// <param name="error">Output parameter containing error message if validation fails.</param>
    /// <returns>True if the request is valid; otherwise, false.</returns>
    public static bool IsValidJsonRpcRequest(JsonRpcRequest request, out string? error)
    {
        error = null;

        if (request == null)
        {
            error = "Request cannot be null";
            return false;
        }

        if (request.JsonRpc != "2.0")
        {
            error = $"Invalid JSON-RPC version: {request.JsonRpc}. Expected: 2.0";
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.Method))
        {
            error = "Method is required";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets server information for initialize response.
    /// </summary>
    /// <returns>Server information including name, version, and protocol version.</returns>
    public virtual ServerInfo GetServerInfo()
    {
        return new ServerInfo
        {
            Name = settings.Server.Name,
            Version = settings.Server.Version,
            ProtocolVersion = settings.Server.ProtocolVersion,
        };
    }

    /// <summary>
    /// Creates a JSON-RPC 2.0 success response.
    /// </summary>
    /// <param name="result">The result object to include in the response.</param>
    /// <param name="id">The request identifier.</param>
    /// <returns>A JSON-RPC success response.</returns>
    protected JsonRpcResponse CreateSuccessResponse(object? result, object id)
    {
        return new JsonRpcResponse
        {
            JsonRpc = "2.0",
            Result = result,
            Id = id
        };
    }

    /// <summary>
    /// Creates a JSON-RPC 2.0 error response.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="data">Additional error data.</param>
    /// <param name="id">The request identifier.</param>
    /// <returns>A JSON-RPC error response.</returns>
    protected JsonRpcResponse CreateErrorResponse(int code, string message, object? data, object id)
    {
        return new JsonRpcResponse
        {
            JsonRpc = "2.0",
            Error = new JsonRpcError
            {
                Code = code,
                Message = message,
                Data = data
            },
            Id = id,
        };
    }
}
