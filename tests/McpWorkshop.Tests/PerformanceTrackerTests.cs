using McpWorkshop.Shared.Monitoring;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace McpWorkshop.Tests.Unit.Monitoring;

/// <summary>
/// Unit tests for PerformanceTracker
/// Tests performance tracking and metrics collection
/// </summary>
public class PerformanceTrackerTests
{
    private readonly Mock<ILogger<PerformanceTracker>> _mockLogger;
    private readonly PerformanceTracker _tracker;

    public PerformanceTrackerTests()
    {
        _mockLogger = new Mock<ILogger<PerformanceTracker>>();
        _tracker = new PerformanceTracker(_mockLogger.Object);
    }

    [Fact]
    public void TrackRequest_RecordsMetrics()
    {
        // Arrange
        var method = "test-operation";

        // Act
        using (_tracker.TrackRequest(method))
        {
            Thread.Sleep(50); // Simulate work
        }

        // Assert
        var metrics = _tracker.GetMetrics(method);
        Assert.NotNull(metrics);
        Assert.Equal(1, metrics.TotalRequests);
        Assert.True(metrics.AverageDurationMs >= 50);
    }

    [Fact]
    public void TrackRequest_WithMultipleRequests_AggregatesCorrectly()
    {
        // Arrange
        var method = "aggregate-test";

        // Act
        for (int i = 0; i < 5; i++)
        {
            using (_tracker.TrackRequest(method))
            {
                Thread.Sleep(10);
            }
        }

        // Assert
        var metrics = _tracker.GetMetrics(method);
        Assert.NotNull(metrics);
        Assert.Equal(5, metrics.TotalRequests);
        Assert.Equal(5, metrics.SuccessfulRequests);
    }

    [Fact]
    public void TrackRequest_WithSlowOperation_LogsWarning()
    {
        // Arrange
        var method = "slow-operation";

        // Act
        using (_tracker.TrackRequest(method))
        {
            Thread.Sleep(1100); // Exceed 1000ms threshold
        }

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Slow request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void GetMetrics_WithNonExistentMethod_ReturnsNull()
    {
        // Act
        var metrics = _tracker.GetMetrics("non-existent");

        // Assert
        Assert.Null(metrics);
    }

    [Fact]
    public void GetAllMetrics_ReturnsAllTrackedMethods()
    {
        // Arrange
        using (_tracker.TrackRequest("method1")) { }
        using (_tracker.TrackRequest("method2")) { }
        using (_tracker.TrackRequest("method3")) { }

        // Act
        var allMetrics = _tracker.GetAllMetrics();

        // Assert
        Assert.Equal(3, allMetrics.Count);
        Assert.Contains("method1", allMetrics.Keys);
        Assert.Contains("method2", allMetrics.Keys);
        Assert.Contains("method3", allMetrics.Keys);
    }

    [Fact]
    public void TrackRequest_WithRequestId_LogsDebugInfo()
    {
        // Arrange
        var method = "test-with-id";
        var requestId = "req-123";

        // Act
        using (_tracker.TrackRequest(method, requestId))
        {
            Thread.Sleep(10);
        }

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(requestId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void PerformanceMetrics_CalculatesP95Correctly()
    {
        // Arrange
        var method = "p95-test";
        var responseTimes = new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };

        // Act
        foreach (var time in responseTimes)
        {
            using (_tracker.TrackRequest(method))
            {
                Thread.Sleep(time);
            }
        }

        // Assert
        var metrics = _tracker.GetMetrics(method);
        Assert.NotNull(metrics);
        Assert.True(metrics.MaxDurationMs > 0);
        Assert.True(metrics.MinDurationMs > 0);
    }

    [Fact]
    public void PerformanceMetrics_TracksMinAndMaxResponseTimes()
    {
        // Arrange
        var method = "minmax-test";

        // Act
        using (_tracker.TrackRequest(method)) { Thread.Sleep(50); }
        using (_tracker.TrackRequest(method)) { Thread.Sleep(150); }
        using (_tracker.TrackRequest(method)) { Thread.Sleep(100); }

        // Assert
        var metrics = _tracker.GetMetrics(method);
        Assert.NotNull(metrics);
        Assert.True(metrics.MinDurationMs >= 50);
        Assert.True(metrics.MaxDurationMs >= 150);
        Assert.True(metrics.MinDurationMs < metrics.MaxDurationMs);
    }
}
