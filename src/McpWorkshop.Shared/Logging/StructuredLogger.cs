using System.Text.Json;

using Microsoft.Extensions.Logging;

namespace McpWorkshop.Shared.Logging;

/// <summary>
/// Concrete implementation of structured logging with sensitive field redaction.
/// </summary>
public class StructuredLogger : IStructuredLogger
{
    private readonly ILogger logger;

    private readonly string[] sensitiveFields = { "password", "token", "secret", "apiKey", "connectionString" };

    /// <summary>
    /// Initializes a new instance of the <see cref="StructuredLogger"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public StructuredLogger(ILogger<StructuredLogger> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public void LogRequest(string method, string requestId, IDictionary<string, object>? parameters = null)
    {
        var sanitizedParams = parameters != null ? RedactSensitiveFields(parameters) : null;

        logger.LogInformation(
            "MCP Request | Method: {Method} | RequestId: {RequestId} | Parameters: {Parameters} | Timestamp: {Timestamp}",
            method,
            requestId,
            sanitizedParams != null ? JsonSerializer.Serialize(sanitizedParams) : "none",
            DateTime.UtcNow);
    }

    /// <inheritdoc/>
    public void LogResponse(string method, string requestId, long durationMs, int statusCode)
    {
        var logLevel = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;

        logger.Log(
            logLevel,
            "MCP Response | Method: {Method} | RequestId: {RequestId} | Duration: {DurationMs}ms | StatusCode: {StatusCode} | Timestamp: {Timestamp}",
            method,
            requestId,
            durationMs,
            statusCode,
            DateTime.UtcNow);
    }

    /// <inheritdoc/>
    public void LogError(string method, string requestId, Exception exception, IDictionary<string, object>? context = null)
    {
        var sanitizedContext = context != null ? RedactSensitiveFields(context) : null;

        logger.LogError(
            exception,
            "MCP Error | Method: {Method} | RequestId: {RequestId} | Context: {Context} | Timestamp: {Timestamp}",
            method,
            requestId,
            sanitizedContext != null ? JsonSerializer.Serialize(sanitizedContext) : "none",
            DateTime.UtcNow);
    }

    /// <inheritdoc/>
    public void LogToolExecution(string toolName, string requestId, IDictionary<string, object> parameters, long durationMs, bool success)
    {
        var sanitizedParams = RedactSensitiveFields(parameters);

        logger.LogInformation(
            "MCP Tool Execution | Tool: {ToolName} | RequestId: {RequestId} | Parameters: {Parameters} | Duration: {DurationMs}ms | Success: {Success} | Timestamp: {Timestamp}",
            toolName,
            requestId,
            JsonSerializer.Serialize(sanitizedParams),
            durationMs,
            success,
            DateTime.UtcNow);
    }

    /// <inheritdoc/>
    public void LogResourceAccess(string resourceUri, string requestId, long durationMs, bool success)
    {
        logger.LogInformation(
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
    /// <param name="fields">The dictionary of fields to sanitize.</param>
    /// <returns>A new dictionary with sensitive values redacted.</returns>
    private IDictionary<string, object> RedactSensitiveFields(IDictionary<string, object> fields)
    {
        var redacted = new Dictionary<string, object>(fields.Count);

        foreach (var kvp in fields)
        {
            var key = kvp.Key.ToLowerInvariant();
            var isSensitive = sensitiveFields.Any(sf => key.Contains(sf.ToLowerInvariant()));

            redacted[kvp.Key] = isSensitive ? "[REDACTED]" : kvp.Value;
        }

        return redacted;
    }
}
