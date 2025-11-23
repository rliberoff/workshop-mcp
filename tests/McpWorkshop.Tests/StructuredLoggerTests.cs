using McpWorkshop.Shared.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace McpWorkshop.Tests.Unit.Logging;

/// <summary>
/// Unit tests for StructuredLogger
/// Tests structured logging with sensitive field redaction
/// </summary>
public class StructuredLoggerTests
{
    private readonly Mock<ILogger<StructuredLogger>> _mockLogger;
    private readonly StructuredLogger _logger;

    public StructuredLoggerTests()
    {
        _mockLogger = new Mock<ILogger<StructuredLogger>>();
        _logger = new StructuredLogger(_mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StructuredLogger(null!));
    }

    [Fact]
    public void LogRequest_WithMethodAndRequestId_LogsInformation()
    {
        // Arrange
        var method = "resources/list";
        var requestId = "req-123";

        // Act
        _logger.LogRequest(method, requestId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(method) && v.ToString()!.Contains(requestId)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogRequest_WithParameters_IncludesParametersInLog()
    {
        // Arrange
        var method = "tools/call";
        var requestId = "req-456";
        var parameters = new Dictionary<string, object>
        {
            ["toolName"] = "GetCustomers",
            ["page"] = 1
        };

        // Act
        _logger.LogRequest(method, requestId, parameters);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GetCustomers")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogRequest_WithSensitiveFields_RedactsPassword()
    {
        // Arrange
        var method = "auth/login";
        var requestId = "req-789";
        var parameters = new Dictionary<string, object>
        {
            ["username"] = "user",
            ["password"] = "secret123"
        };

        // Act
        _logger.LogRequest(method, requestId, parameters);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => !v.ToString()!.Contains("secret123")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogResponse_WithSuccessStatusCode_LogsInformation()
    {
        // Arrange
        var method = "resources/read";
        var requestId = "req-200";
        var durationMs = 150L;
        var statusCode = 200;

        // Act
        _logger.LogResponse(method, requestId, durationMs, statusCode);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(method) && v.ToString()!.Contains("150")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogResponse_WithErrorStatusCode_LogsWarning()
    {
        // Arrange
        var method = "resources/read";
        var requestId = "req-500";
        var durationMs = 50L;
        var statusCode = 500;

        // Act
        _logger.LogResponse(method, requestId, durationMs, statusCode);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogError_WithException_LogsError()
    {
        // Arrange
        var method = "tools/call";
        var requestId = "req-error";
        var exception = new InvalidOperationException("Test error");

        // Act
        _logger.LogError(method, requestId, exception);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(method)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogError_WithContext_IncludesContextInLog()
    {
        // Arrange
        var method = "resources/read";
        var requestId = "req-ctx";
        var exception = new Exception("Error");
        var context = new Dictionary<string, object>
        {
            ["resourceUri"] = "workshop://customers",
            ["userId"] = "user-123"
        };

        // Act
        _logger.LogError(method, requestId, exception, context);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("workshop://customers")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogToolExecution_WithSuccess_LogsInformation()
    {
        // Arrange
        var toolName = "GetCustomers";
        var requestId = "req-tool";
        var parameters = new Dictionary<string, object> { ["page"] = 1 };
        var durationMs = 250L;
        var success = true;

        // Act
        _logger.LogToolExecution(toolName, requestId, parameters, durationMs, success);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(toolName) && v.ToString()!.Contains("250")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogResourceAccess_WithSuccess_LogsInformation()
    {
        // Arrange
        var resourceUri = "workshop://products";
        var requestId = "req-resource";
        var durationMs = 100L;
        var success = true;

        // Act
        _logger.LogResourceAccess(resourceUri, requestId, durationMs, success);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(resourceUri)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
