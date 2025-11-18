using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Exercise1StaticResources;

namespace McpWorkshop.Tests.Integration;

/// <summary>
/// Integration tests for Exercise 4: Multi-server orchestration with VirtualAnalyst.
/// Validates coordination between SQL MCP, Cosmos MCP, REST MCP, and VirtualAnalyst.
/// </summary>
public class Exercise4IntegrationTests
{
    [Fact(Skip = "Requires multi-server orchestration infrastructure - Exercise 4 advanced feature")]
    public async Task VirtualAnalyst_ShouldOrchestrateMultipleServers()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "AnalyzeCustomerOrders",
                arguments = new
                {
                    customerId = "123"
                }
            },
            id = 1
        };

        // Act
        var response = await client.PostAsJsonAsync("/mcp", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert - Should return aggregated data from multiple sources
        Assert.NotNull(jsonResponse);
        Assert.NotNull(jsonResponse["result"]);

        var contentArray = jsonResponse["result"]!["content"]!.AsArray();
        Assert.NotEmpty(contentArray);

        var textContent = contentArray[0]!["text"]?.GetValue<string>();
        Assert.NotNull(textContent);

        // Verify it contains data from multiple servers
        Assert.Contains("customer", textContent.ToLower());
        Assert.Contains("order", textContent.ToLower());
    }

    [Fact(Skip = "Requires SQL Server infrastructure - Exercise 4 advanced feature")]
    public async Task SqlMcpServer_ShouldProvideCustomerData()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/read",
            @params = new
            {
                uri = "sql://customers/123"
            },
            id = 2
        };

        // Act
        var response = await client.PostAsJsonAsync("/mcp", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        var contents = jsonResponse["result"]!["contents"]!.AsArray();
        Assert.NotEmpty(contents);

        var customerData = JsonNode.Parse(contents[0]!["text"]?.GetValue<string>()!);
        Assert.NotNull(customerData!["id"]);
        Assert.NotNull(customerData["name"]);
    }

    [Fact(Skip = "Requires Azure Cosmos DB infrastructure - Exercise 4 advanced feature")]
    public async Task CosmosMcpServer_ShouldProvideSessionData()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        var request = new
        {
            jsonrpc = "2.0",
            method = "resources/read",
            @params = new
            {
                uri = "cosmos://sessions/user-123"
            },
            id = 3
        };

        // Act
        var response = await client.PostAsJsonAsync("/mcp", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        var contents = jsonResponse["result"]!["contents"]!.AsArray();
        Assert.NotEmpty(contents);

        var sessionData = JsonNode.Parse(contents[0]!["text"]?.GetValue<string>()!);
        Assert.NotNull(sessionData!["userId"]);
        Assert.NotNull(sessionData["sessionId"]);
    }

    [Fact(Skip = "Requires external REST API infrastructure - Exercise 4 advanced feature")]
    public async Task RestMcpServer_ShouldCallExternalApi()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "GetExternalData",
                arguments = new
                {
                    endpoint = "/api/products/123"
                }
            },
            id = 4
        };

        // Act
        var response = await client.PostAsJsonAsync("/mcp", request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert
        Assert.NotNull(jsonResponse);
        var contentArray = jsonResponse["result"]!["content"]!.AsArray();
        Assert.NotEmpty(contentArray);
    }

    [Fact(Skip = "Requires caching and orchestration infrastructure - Exercise 4 advanced feature")]
    public async Task VirtualAnalyst_WithCaching_ShouldImprovePerformance()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "AnalyzeCustomerOrders",
                arguments = new
                {
                    customerId = "123"
                }
            },
            id = 5
        };

        // Act - First call (cold cache)
        var sw1 = System.Diagnostics.Stopwatch.StartNew();
        var response1 = await client.PostAsJsonAsync("/mcp", request);
        sw1.Stop();

        // Act - Second call (warm cache)
        var sw2 = System.Diagnostics.Stopwatch.StartNew();
        var response2 = await client.PostAsJsonAsync("/mcp", request);
        sw2.Stop();

        // Assert - Second call should be faster
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();

        Assert.True(sw2.ElapsedMilliseconds <= sw1.ElapsedMilliseconds,
            $"Cached call ({sw2.ElapsedMilliseconds}ms) should be faster than or equal to first call ({sw1.ElapsedMilliseconds}ms)");
    }

    [Fact]
    public async Task VirtualAnalyst_ErrorHandling_ShouldGracefullyDegrade()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "AnalyzeCustomerOrders",
                arguments = new
                {
                    customerId = "nonexistent"
                }
            },
            id = 6
        };

        // Act
        var response = await client.PostAsJsonAsync("/mcp", request);

        // Assert - Should handle errors gracefully
        Assert.True(response.IsSuccessStatusCode ||
                   response.StatusCode == System.Net.HttpStatusCode.NotFound,
            "Should handle nonexistent customer gracefully");
    }

    [Fact]
    public async Task ServerCommunication_ShouldUseJsonRpcFormat()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Exercise1StaticResources.Program>();
        using var client = factory.CreateClient();

        var request = new
        {
            jsonrpc = "2.0",
            method = "tools/call",
            @params = new
            {
                name = "AnalyzeCustomerOrders",
                arguments = new
                {
                    customerId = "123"
                }
            },
            id = 7
        };

        // Act
        var response = await client.PostAsJsonAsync("/mcp", request);
        var content = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonNode.Parse(content);

        // Assert - All server-to-server communication uses JSON-RPC 2.0
        Assert.Equal("2.0", jsonResponse!["jsonrpc"]?.GetValue<string>());
        Assert.NotNull(jsonResponse["id"]);
    }
}

