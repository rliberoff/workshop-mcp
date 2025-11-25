namespace McpWorkshop.Shared.Logging;

/// <summary>
/// Provides structured logging capabilities for MCP workshop servers.
/// Implements observability patterns required by Constitution Principle V.
/// </summary>
public interface IStructuredLogger
{
    /// <summary>
    /// Logs an incoming MCP request.
    /// </summary>
    /// <param name="method">The method name being invoked.</param>
    /// <param name="requestId">The unique request identifier.</param>
    /// <param name="parameters">Optional dictionary of request parameters.</param>
    void LogRequest(string method, string requestId, IDictionary<string, object>? parameters = null);

    /// <summary>
    /// Logs an MCP response.
    /// </summary>
    /// <param name="method">The method name that was invoked.</param>
    /// <param name="requestId">The unique request identifier.</param>
    /// <param name="durationMs">The request duration in milliseconds.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    void LogResponse(string method, string requestId, long durationMs, int statusCode);

    /// <summary>
    /// Logs an error that occurred during request processing.
    /// </summary>
    /// <param name="method">The method name where the error occurred.</param>
    /// <param name="requestId">The unique request identifier.</param>
    /// <param name="exception">The exception that was thrown.</param>
    /// <param name="context">Optional dictionary of contextual information.</param>
    void LogError(string method, string requestId, Exception exception, IDictionary<string, object>? context = null);

    /// <summary>
    /// Logs the execution of an MCP tool.
    /// </summary>
    /// <param name="toolName">The name of the tool being executed.</param>
    /// <param name="requestId">The unique request identifier.</param>
    /// <param name="parameters">Dictionary of tool parameters.</param>
    /// <param name="durationMs">The tool execution duration in milliseconds.</param>
    /// <param name="success">Indicates whether the tool execution was successful.</param>
    void LogToolExecution(string toolName, string requestId, IDictionary<string, object> parameters, long durationMs, bool success);

    /// <summary>
    /// Logs access to an MCP resource.
    /// </summary>
    /// <param name="resourceUri">The URI of the resource being accessed.</param>
    /// <param name="requestId">The unique request identifier.</param>
    /// <param name="durationMs">The resource access duration in milliseconds.</param>
    /// <param name="success">Indicates whether the resource access was successful.</param>
    void LogResourceAccess(string resourceUri, string requestId, long durationMs, bool success);
}
