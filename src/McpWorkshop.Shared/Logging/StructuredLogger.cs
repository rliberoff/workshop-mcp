using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace McpWorkshop.Shared.Logging;

/// <summary>
/// Provides structured logging capabilities for MCP workshop servers.
/// Implements observability patterns required by Constitution Principle V.
/// </summary>
public interface IStructuredLogger
{
    void LogRequest(string method, string requestId, IDictionary<string, object>? parameters = null);
    void LogResponse(string method, string requestId, long durationMs, int statusCode);
    void LogError(string method, string requestId, Exception exception, IDictionary<string, object>? context = null);
    void LogToolExecution(string toolName, string requestId, IDictionary<string, object> parameters, long durationMs, bool success);
    void LogResourceAccess(string resourceUri, string requestId, long durationMs, bool success);
}

/// <summary>
/// Concrete implementation of structured logging with sensitive field redaction.
/// </summary>
public class StructuredLogger : IStructuredLogger
{
    private readonly ILogger _logger;
    private readonly string[] _sensitiveFields = { "password", "token", "secret", "apiKey", "connectionString" };

    public StructuredLogger(ILogger<StructuredLogger> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogRequest(string method, string requestId, IDictionary<string, object>? parameters = null)
    {
        var sanitizedParams = parameters != null ? RedactSensitiveFields(parameters) : null;

        _logger.LogInformation(
            "MCP Request | Method: {Method} | RequestId: {RequestId} | Parameters: {Parameters} | Timestamp: {Timestamp}",
            method,
            requestId,
            sanitizedParams != null ? JsonSerializer.Serialize(sanitizedParams) : "none",
            DateTime.UtcNow);
    }

    public void LogResponse(string method, string requestId, long durationMs, int statusCode)
    {
        var logLevel = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        _logger.Log(
            logLevel,
            "MCP Response | Method: {Method} | RequestId: {RequestId} | Duration: {DurationMs}ms | StatusCode: {StatusCode} | Timestamp: {Timestamp}",
            method,
            requestId,
            durationMs,
            statusCode,
            DateTime.UtcNow);
    }

    public void LogError(string method, string requestId, Exception exception, IDictionary<string, object>? context = null)
    {
        var sanitizedContext = context != null ? RedactSensitiveFields(context) : null;

        _logger.LogError(
            exception,
            "MCP Error | Method: {Method} | RequestId: {RequestId} | Context: {Context} | Timestamp: {Timestamp}",
            method,
            requestId,
            sanitizedContext != null ? JsonSerializer.Serialize(sanitizedContext) : "none",
            DateTime.UtcNow);
    }

    public void LogToolExecution(string toolName, string requestId, IDictionary<string, object> parameters, long durationMs, bool success)
    {
        var sanitizedParams = RedactSensitiveFields(parameters);

        _logger.LogInformation(
            "MCP Tool Execution | Tool: {ToolName} | RequestId: {RequestId} | Parameters: {Parameters} | Duration: {DurationMs}ms | Success: {Success} | Timestamp: {Timestamp}",
            toolName,
            requestId,
            JsonSerializer.Serialize(sanitizedParams),
            durationMs,
            success,
            DateTime.UtcNow);
    }

    public void LogResourceAccess(string resourceUri, string requestId, long durationMs, bool success)
    {
        _logger.LogInformation(
            "MCP Resource Access | Resource: {ResourceUri} | RequestId: {RequestId} | Duration: {DurationMs}ms | Success: {Success} | Timestamp: {Timestamp}",
            resourceUri,
            requestId,
            durationMs,
            success,
            DateTime.UtcNow);
    }

    /// <summary>
    /// Redacts sensitive fields from parameter dictionaries.
    /// Implements security requirement from Exercise 3 contract.
    /// </summary>
    private IDictionary<string, object> RedactSensitiveFields(IDictionary<string, object> fields)
    {
        var redacted = new Dictionary<string, object>(fields.Count);

        foreach (var kvp in fields)
        {
            var key = kvp.Key.ToLowerInvariant();
            var isSensitive = _sensitiveFields.Any(sf => key.Contains(sf.ToLowerInvariant()));

            redacted[kvp.Key] = isSensitive ? "[REDACTED]" : kvp.Value;
        }

        return redacted;
    }
}
