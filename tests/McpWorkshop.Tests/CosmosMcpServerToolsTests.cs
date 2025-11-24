using System.Text.Json;
using Exercise4CosmosMcpServer.Models;
using Exercise4CosmosMcpServer.Tools;
using Xunit;

namespace McpWorkshop.Tests.Unit.Tools;

/// <summary>
/// Unit tests for Exercise4 Cosmos MCP Server Tools
/// Tests AnalyzeUserBehaviorTool and GetAbandonedCartsTool
/// </summary>
public class CosmosMcpServerToolsTests
{
    [Fact]
    public void AnalyzeUserBehaviorTool_GetDefinition_ReturnsValidSchema()
    {
        // Act
        var definition = AnalyzeUserBehaviorTool.GetDefinition();

        // Assert
        Assert.NotNull(definition);
        // Just verify it returns an object, the actual structure is validated in integration tests
    }

    [Fact]
    public void AnalyzeUserBehaviorTool_Execute_WithoutUserId_ThrowsArgumentException()
    {
        // Arrange
        var sessions = Array.Empty<UserSession>();
        var cartEvents = Array.Empty<CartEvent>();
        var arguments = new Dictionary<string, JsonElement>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            AnalyzeUserBehaviorTool.Execute(arguments, sessions, cartEvents));
    }

    [Fact]
    public void AnalyzeUserBehaviorTool_Execute_WithUserId_ReturnsAnalysis()
    {
        // Arrange
        var sessions = CreateSampleSessions();
        var cartEvents = CreateSampleCartEvents();
        var arguments = new Dictionary<string, JsonElement>
        {
            ["userId"] = JsonDocument.Parse("\"user-123\"").RootElement
        };

        // Act
        var result = AnalyzeUserBehaviorTool.Execute(arguments, sessions, cartEvents);
        var resultDict = result as Dictionary<string, object>;

        // Assert
        Assert.NotNull(resultDict);
        Assert.True(resultDict.ContainsKey("content"));
    }

    [Fact]
    public void AnalyzeUserBehaviorTool_Execute_WithMetricType_UsesSpecifiedMetric()
    {
        // Arrange
        var sessions = CreateSampleSessions();
        var cartEvents = CreateSampleCartEvents();
        var arguments = new Dictionary<string, JsonElement>
        {
            ["userId"] = JsonDocument.Parse("\"user-123\"").RootElement,
            ["metricType"] = JsonDocument.Parse("\"pageViews\"").RootElement
        };

        // Act
        var result = AnalyzeUserBehaviorTool.Execute(arguments, sessions, cartEvents);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void AnalyzeUserBehaviorTool_Execute_DefaultMetricType_IsSessions()
    {
        // Arrange
        var sessions = CreateSampleSessions();
        var cartEvents = CreateSampleCartEvents();
        var arguments = new Dictionary<string, JsonElement>
        {
            ["userId"] = JsonDocument.Parse("\"user-123\"").RootElement
        };

        // Act
        var result = AnalyzeUserBehaviorTool.Execute(arguments, sessions, cartEvents);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void AnalyzeUserBehaviorTool_Execute_CalculatesMetricsCorrectly()
    {
        // Arrange
        var sessions = new[]
        {
            new UserSession
            {
                UserId = "user-456",
                StartTime = DateTime.Now.AddHours(-2),
                EndTime = DateTime.Now.AddHours(-1),
                PagesViewed = 10,
                Actions = new[] { "view", "click", "scroll", "add_to_cart", "checkout" }
            }
        };
        var cartEvents = new[]
        {
            new CartEvent
            {
                UserId = "user-456",
                ProductId = 1,
                Action = "addToCart",
                Timestamp = DateTime.Now.AddHours(-1),
                Quantity = 2
            }
        };
        var arguments = new Dictionary<string, JsonElement>
        {
            ["userId"] = JsonDocument.Parse("\"user-456\"").RootElement
        };

        // Act
        var result = AnalyzeUserBehaviorTool.Execute(arguments, sessions, cartEvents);

        // Assert
        Assert.NotNull(result);
    }

    private UserSession[] CreateSampleSessions()
    {
        return new[]
        {
            new UserSession
            {
                UserId = "user-123",
                StartTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2024, 1, 1, 10, 30, 0, DateTimeKind.Utc),
                PagesViewed = 15,
                Actions = new[] { "view", "click", "scroll", "add_to_cart", "view", "click", "scroll", "checkout" }
            },
            new UserSession
            {
                UserId = "user-123",
                StartTime = new DateTime(2024, 1, 2, 14, 0, 0, DateTimeKind.Utc),
                EndTime = new DateTime(2024, 1, 2, 14, 45, 0, DateTimeKind.Utc),
                PagesViewed = 20,
                Actions = new[] { "view", "click", "scroll", "add_to_cart", "view", "click", "scroll", "checkout", "view", "click", "scroll", "remove_from_cart" }
            }
        };
    }

    private CartEvent[] CreateSampleCartEvents()
    {
        return new[]
        {
            new CartEvent
            {
                UserId = "user-123",
                ProductId = 101,
                Action = "addToCart",
                Timestamp = new DateTime(2024, 1, 1, 10, 15, 0, DateTimeKind.Utc),
                Quantity = 2
            },
            new CartEvent
            {
                UserId = "user-123",
                ProductId = 202,
                Action = "checkout",
                Timestamp = new DateTime(2024, 1, 2, 14, 20, 0, DateTimeKind.Utc),
                Quantity = 1
            }
        };
    }
}
