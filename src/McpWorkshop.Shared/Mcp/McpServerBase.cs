using McpWorkshop.Shared.Logging;
using Microsoft.Extensions.Options;

namespace McpWorkshop.Shared.Mcp;

/// <summary>
/// Base class for MCP servers providing common functionality.
/// Implements MCP-First design principle (Constitution I).
/// </summary>
public abstract class McpServerBase
{
    protected readonly IStructuredLogger Logger;
    protected readonly Configuration.WorkshopSettings Settings;

    protected McpServerBase(
        IStructuredLogger logger,
        IOptions<Configuration.WorkshopSettings> settings)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Gets server information for initialize response
    /// </summary>
    public virtual ServerInfo GetServerInfo()
    {
        return new ServerInfo
        {
            Name = Settings.Server.Name,
            Version = Settings.Server.Version,
            ProtocolVersion = Settings.Server.ProtocolVersion
        };
    }

    /// <summary>
    /// Generates a unique request ID for tracking
    /// </summary>
    protected string GenerateRequestId()
    {
        return Guid.NewGuid().ToString("N")[..12];
    }

    /// <summary>
    /// Validates JSON-RPC 2.0 request format
    /// </summary>
    protected bool IsValidJsonRpcRequest(JsonRpcRequest request, out string? error)
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
    /// Creates a JSON-RPC 2.0 success response
    /// </summary>
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
    /// Creates a JSON-RPC 2.0 error response
    /// </summary>
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
            Id = id
        };
    }
}

/// <summary>
/// JSON-RPC 2.0 request structure
/// </summary>
public class JsonRpcRequest
{
    public string JsonRpc { get; set; } = "2.0";
    public string Method { get; set; } = string.Empty;
    public object? Params { get; set; }
    public object? Id { get; set; }
}

/// <summary>
/// JSON-RPC 2.0 response structure
/// </summary>
public class JsonRpcResponse
{
    public string JsonRpc { get; set; } = "2.0";
    public object? Result { get; set; }
    public JsonRpcError? Error { get; set; }
    public object? Id { get; set; }
}

/// <summary>
/// JSON-RPC 2.0 error structure
/// </summary>
public class JsonRpcError
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

/// <summary>
/// MCP server information
/// </summary>
public class ServerInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string ProtocolVersion { get; set; } = string.Empty;
    public ServerCapabilities? Capabilities { get; set; }
}

/// <summary>
/// MCP server capabilities
/// </summary>
public class ServerCapabilities
{
    public ResourceCapabilities? Resources { get; set; }
    public ToolCapabilities? Tools { get; set; }
    public LoggingCapabilities? Logging { get; set; }
}

public class ResourceCapabilities
{
    public bool Subscribe { get; set; } = false;
    public bool ListChanged { get; set; } = false;
}

public class ToolCapabilities
{
    public bool ListChanged { get; set; } = false;
}

public class LoggingCapabilities
{
    public string[] Levels { get; set; } = { "DEBUG", "INFO", "WARN", "ERROR" };
}

/// <summary>
/// Standard JSON-RPC 2.0 error codes
/// </summary>
public static class JsonRpcErrorCodes
{
    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InvalidParams = -32602;
    public const int InternalError = -32603;

    // Custom MCP error codes (from contracts)
    public const int Unauthorized = -32001;
    public const int Forbidden = -32002;
    public const int RateLimitExceeded = -32003;
}
