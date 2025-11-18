using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace McpWorkshop.Shared.Monitoring;

/// <summary>
/// Tracks performance metrics for MCP server operations
/// </summary>
public class PerformanceTracker
{
    private readonly ConcurrentDictionary<string, PerformanceMetrics> _metrics = new();
    private readonly ILogger<PerformanceTracker> _logger;

    public PerformanceTracker(ILogger<PerformanceTracker> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Start tracking a request
    /// </summary>
    public IDisposable TrackRequest(string method, string? id = null)
    {
        return new RequestTracker(this, method, id);
    }

    /// <summary>
    /// Record a request completion
    /// </summary>
    internal void RecordRequest(string method, long elapsedMs, bool success)
    {
        var metrics = _metrics.GetOrAdd(method, _ => new PerformanceMetrics());
        metrics.RecordRequest(elapsedMs, success);

        if (elapsedMs > 1000)
        {
            _logger.LogWarning("[PERFORMANCE] Slow request: {Method} took {ElapsedMs}ms", method, elapsedMs);
        }
    }

    /// <summary>
    /// Get metrics for a specific method
    /// </summary>
    public PerformanceMetrics? GetMetrics(string method)
    {
        _metrics.TryGetValue(method, out var metrics);
        return metrics;
    }

    /// <summary>
    /// Get all tracked metrics
    /// </summary>
    public Dictionary<string, PerformanceMetrics> GetAllMetrics()
    {
        return _metrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    /// <summary>
    /// Request tracker disposable
    /// </summary>
    private class RequestTracker : IDisposable
    {
        private readonly PerformanceTracker _tracker;
        private readonly string _method;
        private readonly string? _id;
        private readonly Stopwatch _stopwatch;
        private bool _success = true;

        public RequestTracker(PerformanceTracker tracker, string method, string? id)
        {
            _tracker = tracker;
            _method = method;
            _id = id;
            _stopwatch = Stopwatch.StartNew();
        }

        public void MarkFailure()
        {
            _success = false;
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            _tracker.RecordRequest(_method, _stopwatch.ElapsedMilliseconds, _success);
            _tracker._logger.LogDebug(
                "[METRICS] {Method} (id={Id}) completed in {ElapsedMs}ms (success={Success})",
                _method, _id ?? "none", _stopwatch.ElapsedMilliseconds, _success);
        }
    }
}

/// <summary>
/// Performance metrics for a specific operation
/// </summary>
public class PerformanceMetrics
{
    private long _totalRequests;
    private long _successfulRequests;
    private long _failedRequests;
    private long _totalDurationMs;
    private long _minDurationMs = long.MaxValue;
    private long _maxDurationMs;
    private readonly object _lock = new();

    public long TotalRequests => _totalRequests;
    public long SuccessfulRequests => _successfulRequests;
    public long FailedRequests => _failedRequests;
    public double AverageDurationMs => _totalRequests > 0 ? (double)_totalDurationMs / _totalRequests : 0;
    public long MinDurationMs => _minDurationMs == long.MaxValue ? 0 : _minDurationMs;
    public long MaxDurationMs => _maxDurationMs;
    public double SuccessRate => _totalRequests > 0 ? (double)_successfulRequests / _totalRequests * 100 : 0;

    internal void RecordRequest(long durationMs, bool success)
    {
        lock (_lock)
        {
            _totalRequests++;
            _totalDurationMs += durationMs;

            if (success)
                _successfulRequests++;
            else
                _failedRequests++;

            if (durationMs < _minDurationMs)
                _minDurationMs = durationMs;

            if (durationMs > _maxDurationMs)
                _maxDurationMs = durationMs;
        }
    }

    public override string ToString()
    {
        return $"Requests: {TotalRequests} | Success: {SuccessfulRequests} ({SuccessRate:F1}%) | " +
               $"Avg: {AverageDurationMs:F1}ms | Min: {MinDurationMs}ms | Max: {MaxDurationMs}ms";
    }
}
