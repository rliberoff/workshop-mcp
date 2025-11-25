namespace McpWorkshop.Shared.Monitoring;

/// <summary>
/// Performance metrics for a specific operation.
/// </summary>
public class PerformanceMetrics
{
    private readonly object @lock = new();

    private long totalRequests;

    private long successfulRequests;

    private long failedRequests;

    private long totalDurationMs;

    private long minDurationMs = long.MaxValue;

    private long maxDurationMs;

    /// <summary>
    /// Gets the total number of requests.
    /// </summary>
    public long TotalRequests => totalRequests;

    /// <summary>
    /// Gets the number of successful requests.
    /// </summary>
    public long SuccessfulRequests => successfulRequests;

    /// <summary>
    /// Gets the number of failed requests.
    /// </summary>
    public long FailedRequests => failedRequests;

    /// <summary>
    /// Gets the average duration of requests in milliseconds.
    /// </summary>
    public double AverageDurationMs => totalRequests > 0 ? (double)totalDurationMs / totalRequests : 0;

    /// <summary>
    /// Gets the minimum duration of a request in milliseconds.
    /// </summary>
    public long MinDurationMs => minDurationMs == long.MaxValue ? 0 : minDurationMs;

    /// <summary>
    /// Gets the maximum duration of a request in milliseconds.
    /// </summary>
    public long MaxDurationMs => maxDurationMs;

    /// <summary>
    /// Gets the success rate as a percentage.
    /// </summary>
    public double SuccessRate => totalRequests > 0 ? (double)successfulRequests / totalRequests * 100 : 0;

    /// <summary>
    /// Returns a string representation of the performance metrics.
    /// </summary>
    /// <returns>A formatted string containing the metrics summary.</returns>
    public override string ToString()
    {
        return $"Requests: {TotalRequests} | Success: {SuccessfulRequests} ({SuccessRate:F1}%) | " +
               $"Avg: {AverageDurationMs:F1}ms | Min: {MinDurationMs}ms | Max: {MaxDurationMs}ms";
    }

    internal void RecordRequest(long durationMs, bool success)
    {
        lock (@lock)
        {
            totalRequests++;
            totalDurationMs += durationMs;

            if (success)
            {
                successfulRequests++;
            }
            else
            {
                failedRequests++;
            }

            if (durationMs < minDurationMs)
            {
                minDurationMs = durationMs;
            }

            if (durationMs > maxDurationMs)
            {
                maxDurationMs = durationMs;
            }
        }
    }
}
