using System.Collections.Concurrent;
using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace McpWorkshop.Shared.Monitoring;

/// <summary>
/// Tracks performance metrics for MCP server operations.
/// </summary>
public class PerformanceTracker
{
    private readonly ConcurrentDictionary<string, PerformanceMetrics> metrics = new();

    private readonly ILogger<PerformanceTracker> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceTracker"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public PerformanceTracker(ILogger<PerformanceTracker> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// Start tracking a request.
    /// </summary>
    /// <param name="method">The method name being tracked.</param>
    /// <param name="id">Optional request identifier.</param>
    /// <returns>A disposable tracker instance.</returns>
    public IDisposable TrackRequest(string method, string? id = null)
    {
        return new RequestTracker(this, method, id);
    }

    /// <summary>
    /// Get metrics for a specific method.
    /// </summary>
    /// <param name="method">The method name.</param>
    /// <returns>The performance metrics for the specified method, or null if not found.</returns>
    public PerformanceMetrics? GetMetrics(string method)
    {
        metrics.TryGetValue(method, out var aux);
        return aux;
    }

    /// <summary>
    /// Get all tracked metrics.
    /// </summary>
    /// <returns>A dictionary of all tracked metrics by method name.</returns>
    public Dictionary<string, PerformanceMetrics> GetAllMetrics()
    {
        return metrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Record a request completion.
    /// </summary>
    /// <param name="method">The method name.</param>
    /// <param name="elapsedMs">The elapsed time in milliseconds.</param>
    /// <param name="success">Indicates whether the request was successful.</param>
    internal void RecordRequest(string method, long elapsedMs, bool success)
    {
        var aux = metrics.GetOrAdd(method, _ => new PerformanceMetrics());
        aux.RecordRequest(elapsedMs, success);

        if (elapsedMs > 1000)
        {
            logger.LogWarning("[PERFORMANCE] Slow request: {Method} took {ElapsedMs}ms", method, elapsedMs);
        }
    }

    /// <summary>
    /// Request tracker disposable.
    /// </summary>
    private sealed class RequestTracker : IDisposable
    {
        private readonly PerformanceTracker tracker;

        private readonly string method;

        private readonly string? id;

        private readonly Stopwatch stopwatch;

        public RequestTracker(PerformanceTracker tracker, string method, string? id)
        {
            this.tracker = tracker;
            this.method = method;
            this.id = id;
            stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Stops the stopwatch and records the request metrics upon disposal.
        /// </summary>
        public void Dispose()
        {
            stopwatch.Stop();
            tracker.RecordRequest(method, stopwatch.ElapsedMilliseconds, true);
            tracker.logger.LogDebug(
                "[METRICS] {Method} (id={Id}) completed in {ElapsedMs}ms (success={Success})",
                method, id ?? "none", stopwatch.ElapsedMilliseconds, true);
        }
    }
}
